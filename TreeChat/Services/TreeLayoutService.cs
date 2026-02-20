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

        private static double CalculateWidthOfSubtree(TreeNodeViewModel currentNode)
        {
            currentNode.SubtreeWidth.Clear();

            if (currentNode.Children.Count == 0)
                return TreeNodeViewModel.WIDTH;

            double totalWidth = 0;
            foreach (TreeNodeViewModel childNode in currentNode.Children)
            {
                double subWidth = CalculateWidthOfSubtree(childNode);
                currentNode.SubtreeWidth.Add(subWidth);
                totalWidth += subWidth + HorizontalSpacing;
            }
            totalWidth -= HorizontalSpacing;
            return totalWidth;
        }

        private static void UpdateWidthOfTree(TreeNodeViewModel updateNode)
        {
            updateNode.SubtreeWidth.Clear();
            double totalWidth = CalculateWidthOfSubtree(updateNode);

            TreeNodeViewModel currentNode = updateNode;
            TreeNodeViewModel? parentNode = currentNode.ParentNode;
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

        private static void CalculatePositionOfSubtreeRoot(TreeNodeViewModel rootViewModel, double x, double y)
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
            rootViewModel.X = x + totalWidth / 2 - TreeNodeViewModel.WIDTH / 2;
        }

        public static void LayoutTree(TreeNodeViewModel rootNode)
        {
            CalculateWidthOfSubtree(rootNode);
            LayoutSubtree(rootNode, 0, 0);
        }

        public static void UpdateLayoutTree(TreeNodeViewModel updateNode)
        {
            UpdateWidthOfTree(updateNode);
            TreeNodeViewModel currentNode = updateNode;
            while (currentNode.ParentNode != null)
                currentNode = currentNode.ParentNode;
            LayoutSubtree(currentNode, 0, 0);
        }

        private static void LayoutSubtree(TreeNodeViewModel currentNode, double x, double y)
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
