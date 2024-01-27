using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Tree
{
    public TreeNode<string> Root { get; set; }

    public Tree(string data)
    {
        Root = new TreeNode<string>(data);
    }

    // Add a move to the tree if it doesn't exist and return the node
    public TreeNode<string> AddChild(TreeNode<string> parent, string childData)
    {
        TreeNode<string> newChild = new TreeNode<string>(childData);

        if (parent.Child == null)
        {
            parent.Child = newChild;
            return newChild;
        }
        else
        {
            TreeNode<string> currentSibling = parent.Child;
            while (currentSibling.Sibling != null)
            {
                if(string.Equals(currentSibling.Data, newChild.Data))
                {
                    return currentSibling;
                }
                currentSibling = currentSibling.Sibling;
            }
            // Check the last sibling
            if (string.Equals(currentSibling.Data, newChild.Data))
            {
                return currentSibling;  // Node with the same data already exists
            }

            // No existing node found, add the new node as a sibling
            currentSibling.Sibling = newChild;
            return newChild;
        }
    }

    public void PrintTree(TreeNode<string> root, int depth = 0, int sibling = 0)
    {
        if (root != null)
        {
            Debug.Log("depth: " + depth + " sibling: " + sibling + " data: " + root.Data);
            PrintTree(root.Child, depth + 1);
            PrintTree(root.Sibling, depth, sibling + 1);
        }
    }
}
