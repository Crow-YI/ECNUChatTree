using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using TreeChat.Models;
using TreeChat.Services;
using TreeChat.ViewModels;

namespace TreeChat.Views
{
    public partial class TreeVisualizationView : UserControl
    {
        private ChatTree _chatTree;
        private TreeNodeViewModel _rootViewModel;
        private Dictionary<int, FrameworkElement> _nodeElements = new Dictionary<int, FrameworkElement>();

        //用于平移的变量
        private Point _lastMousePosition;
        private bool _isDragging = false;

        //用于缩放的变量
        private double _zoomFactor = 1.0;
        private const double ZoomStep = 0.1;
        private Point _zoomCenter;

        //选中节点
        public static readonly DependencyProperty SelectedNodeProperty =
            DependencyProperty.Register("SelectedNode", typeof(TreeNodeViewModel), typeof(TreeVisualizationView),
                new PropertyMetadata(null, OnSelectedNodeChanged));

        public TreeNodeViewModel SelectedNode
        {
            get => (TreeNodeViewModel)GetValue(SelectedNodeProperty);
            set => SetValue(SelectedNodeProperty, value);
        }

        private static void OnSelectedNodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TreeVisualizationView view && e.NewValue is TreeNodeViewModel node)
            {
                view.HighlightSelectedNode(node);
            }
        }

        public TreeVisualizationView()
        {
            InitializeComponent();
        }

        // 鼠标拖动平移功能
        private void ScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsClickOnNodeElement(e.OriginalSource as DependencyObject))
            {
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                _zoomCenter = e.GetPosition(treeCanvas);
                e.Handled = true;
                return;
            }

            _isDragging = true;
            _lastMousePosition = e.GetPosition(scrollViewer);
            scrollViewer.CaptureMouse();
            e.Handled = true;
        }

        private void ScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && scrollViewer.IsMouseCaptured)
            {
                Point currentPos = e.GetPosition(scrollViewer);
                double deltaX = currentPos.X - _lastMousePosition.X;
                double deltaY = currentPos.Y - _lastMousePosition.Y;

                scrollViewer.ScrollToHorizontalOffset(
                    scrollViewer.HorizontalOffset - deltaX);
                scrollViewer.ScrollToVerticalOffset(
                    scrollViewer.VerticalOffset - deltaY);

                _lastMousePosition = currentPos;
                e.Handled = true;
            }
        }

        private void ScrollViewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                scrollViewer.ReleaseMouseCapture();
                e.Handled = true;
            }

            if (scrollViewer.IsMouseCaptured)
            {
                scrollViewer.ReleaseMouseCapture();
            }
        }

        // 鼠标滚轮缩放功能
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                // 计算缩放中心点
                var mousePosition = e.GetPosition(treeCanvas);
                var transform = treeCanvas.RenderTransform as MatrixTransform ?? new MatrixTransform();
                var matrix = transform.Matrix;

                // 缩放比例
                double zoomDelta = e.Delta > 0 ? ZoomStep : -ZoomStep;
                double newZoomFactor = Math.Max(0.2, Math.Min(_zoomFactor + zoomDelta, 5.0));

                // 计算平移偏移，使缩放中心保持在鼠标位置
                double zoomRatio = newZoomFactor / _zoomFactor;
                double dx = (1 - zoomRatio) * (mousePosition.X * matrix.M11 + matrix.OffsetX);
                double dy = (1 - zoomRatio) * (mousePosition.Y * matrix.M22 + matrix.OffsetY);

                // 应用新缩放
                _zoomFactor = newZoomFactor;
                matrix.ScaleAt(zoomRatio, zoomRatio, mousePosition.X, mousePosition.Y);
                matrix.Translate(dx * (1 / zoomRatio), dy * (1 / zoomRatio)); // 调整平移

                treeCanvas.RenderTransform = new MatrixTransform(matrix);

                e.Handled = true;
            }
        }

        public void SetTree(ChatTree chatTree, TreeNodeViewModel rootViewModel)
        {
            _chatTree = chatTree;
            _rootViewModel = rootViewModel;
            RenderTree();
        }

        public void RefreshView()
        {
            RenderTree();
        }

        private void RenderTree()
        {
            if (_chatTree == null || _rootViewModel == null || treeCanvas == null)
                return;

            treeCanvas.Children.Clear();
            _nodeElements.Clear();

            // 计算所有节点位置
            TreeLayoutService.LayoutTree(_rootViewModel);

            // 先绘制连接线（在节点下方）
            DrawConnections(_rootViewModel);

            // 再绘制节点
            DrawNodes(_rootViewModel);

            // 设置画布大小
            double maxWidth = _nodeElements.Values
                .Select(el => Canvas.GetLeft(el) + el.ActualWidth)
                .DefaultIfEmpty(800).Max();

            double maxHeight = _nodeElements.Values
                .Select(el => Canvas.GetTop(el) + el.ActualHeight)
                .DefaultIfEmpty(600).Max();

            treeCanvas.Width = maxWidth + 100; // 添加边距
            treeCanvas.Height = maxHeight + 100;
        }

        private void DrawConnections(TreeNodeViewModel node)
        {
            if (node.Children.Count == 0)
                return;

            // 父节点底部中心点
            double parentCenterX = node.X + TreeNodeViewModel.WIDTH / 2;
            double parentBottomY = node.Y + TreeNodeViewModel.HEIGHT;

            foreach (var child in node.Children)
            {
                // 子节点顶部中心点
                double childCenterX = child.X + TreeNodeViewModel.WIDTH / 2;
                double childTopY = child.Y;

                // 绘制连接线
                var line = new Line
                {
                    X1 = parentCenterX,
                    Y1 = parentBottomY,
                    X2 = childCenterX,
                    Y2 = childTopY,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.5
                };
                treeCanvas.Children.Add(line);

                // 递归绘制子节点的连接线
                DrawConnections(child);
            }
        }

        private void DrawNodes(TreeNodeViewModel node)
        {
            // 创建节点UI
            var nodeBorder = new Border
            {
                Width = TreeNodeViewModel.WIDTH,
                Height = TreeNodeViewModel.HEIGHT,
                Background = new SolidColorBrush(Color.FromRgb(220, 230, 240)),
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(3),
                Cursor = Cursors.Hand
            };

            // 节点内容
            var textBlock = new TextBlock
            {
                Text = node.DisplayContent,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5)
            };
            nodeBorder.Child = textBlock;

            // 为节点添加点击事件
            var currentNode = node; // 保存当前节点引用，避免闭包问题
            nodeBorder.PreviewMouseLeftButtonDown += (s, e) =>
            {
                SelectedNode = currentNode; // 触发选中逻辑
                e.Handled = true;           // 标记事件已处理，阻止冒泡到ScrollViewer
            };

            // 定位节点
            Canvas.SetLeft(nodeBorder, node.X);
            Canvas.SetTop(nodeBorder, node.Y);

            // 添加到画布
            treeCanvas.Children.Add(nodeBorder);
            _nodeElements[node.ID] = nodeBorder;

            // 递归处理子节点
            foreach (var child in node.Children)
            {
                DrawNodes(child);
            }
        }

        private void HighlightSelectedNode(TreeNodeViewModel node)
        {
            // 重置所有节点的边框
            foreach (var element in _nodeElements.Values)
            {
                if (element is Border border)
                {
                    border.BorderBrush = Brushes.Gray;
                    border.BorderThickness = new Thickness(1);
                }
            }

            // 高亮选中的节点
            if (_nodeElements.TryGetValue(node.ID, out var selectedElement) && selectedElement is Border selectedBorder)
            {
                selectedBorder.BorderBrush = Brushes.Blue;
                selectedBorder.BorderThickness = new Thickness(2);
            }
        }

        private bool IsClickOnNodeElement(DependencyObject clickedElement)
        {
            if (clickedElement == null) return false;

            // 向上遍历可视化树，查找是否是节点的Border
            while (clickedElement != null)
            {
                // 判断当前元素是否是节点集合中的Border
                if (clickedElement is Border border && _nodeElements.Values.Contains(border))
                {
                    return true;
                }
                // 继续向上找父元素
                clickedElement = VisualTreeHelper.GetParent(clickedElement);
            }
            return false;
        }
    }
}