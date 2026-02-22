using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeChat.Models;
using TreeChat.ViewModels;

namespace TreeChat.Services
{
    public static class TreeLayoutService
    {
        private const double HorizontalSpacing = 40;
        private const double VerticalSpacing = 60;

        private static double CalculateWidthOfSubtree(TreeNodeVM currentNode)
        {
            currentNode.SubtreeWidth.Clear();

            if (currentNode.Children.Count == 0)
                return TreeNodeVM.WIDTH;

            double totalWidth = 0;
            foreach (TreeNodeVM childNode in currentNode.Children)
            {
                double subWidth = CalculateWidthOfSubtree(childNode);
                currentNode.SubtreeWidth.Add(subWidth);
                totalWidth += subWidth + HorizontalSpacing;
            }
            totalWidth -= HorizontalSpacing;
            return totalWidth;
        }

        private static void UpdateWidthOfTree(TreeNodeVM updateNode)
        {
            updateNode.SubtreeWidth.Clear();
            double totalWidth = CalculateWidthOfSubtree(updateNode);

            TreeNodeVM currentNode = updateNode;
            TreeNodeVM? parentNode = currentNode.ParentNode;
            while (parentNode != null)
            {
                int index = parentNode.Children.IndexOf(currentNode);
                parentNode.SubtreeWidth[index] = totalWidth;
                totalWidth = 0;
                foreach (double childWidth in parentNode.SubtreeWidth)
                    totalWidth += childWidth + HorizontalSpacing;
                totalWidth -= HorizontalSpacing;
                currentNode = parentNode;
                parentNode = currentNode.ParentNode;
            }
        }

        private static void CalculatePositionOfSubtreeRoot(TreeNodeVM rootViewModel, double x, double y)
        {
            rootViewModel.Y = y;
            if (rootViewModel.SubtreeWidth.Count == 0)
            {
                rootViewModel.X = x;
                return;
            }

            double totalWidth = 0;
            foreach (double childWidth in rootViewModel.SubtreeWidth)
                totalWidth += childWidth + HorizontalSpacing;
            totalWidth -= HorizontalSpacing;
            rootViewModel.X = x + totalWidth / 2 - TreeNodeVM.WIDTH / 2;
        }

        public static void LayoutTree(TreeNodeVM rootNode)
        {
            CalculateWidthOfSubtree(rootNode);
            LayoutSubtree(rootNode, 0, 0);
        }

        public static void UpdateLayoutTree(TreeNodeVM updateNode)
        {
            UpdateWidthOfTree(updateNode);
            TreeNodeVM currentNode = updateNode;
            while (currentNode.ParentNode != null)
                currentNode = currentNode.ParentNode;
            LayoutSubtree(currentNode, 0, 0);
        }

        private static void LayoutSubtree(TreeNodeVM currentNode, double x, double y)
        {
            CalculatePositionOfSubtreeRoot(currentNode, x, y);
            double offsetX = 0;
            int count = currentNode.Children.Count;
            for(int i = 0; i < count; i++)
            {
                LayoutSubtree(currentNode.Children[i], x + offsetX, y + VerticalSpacing);
                offsetX += HorizontalSpacing + currentNode.SubtreeWidth[i];
            }
        }
    }
}
