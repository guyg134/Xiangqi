 
class TreeNode<T>
{
    public T Data { get; set; }
    public TreeNode<T> Child { get; set; }
    public TreeNode<T> Sibling { get; set; }

    public TreeNode(T data)
    {
        Data = data;
        Child = null;
        Sibling = null;
    }
}