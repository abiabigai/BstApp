namespace BstApp;

/// <summary>
/// Node of the binary search tree (allocated dynamically on the heap).
/// </summary>
public class TreeNode
{
    public int Value;
    public TreeNode? Left;
    public TreeNode? Right;

    public TreeNode(int value)
    {
        Value = value;
    }
}

/// <summary>
/// Binary Search Tree implemented with dynamic memory (linked nodes).
/// Duplicate values are not allowed.
/// </summary>
public class BinarySearchTree
{
    private TreeNode? _root;

    public TreeNode? Root => _root;
    public int Count { get; private set; }

    // ---------------- INSERT ----------------
    /// <returns>true if inserted, false if the value already exists.</returns>
    public bool Insert(int value)
    {
        if (_root == null)
        {
            _root = new TreeNode(value);
            Count++;
            return true;
        }

        TreeNode current = _root;
        while (true)
        {
            if (value == current.Value)
                return false;

            if (value < current.Value)
            {
                if (current.Left == null)
                {
                    current.Left = new TreeNode(value);
                    Count++;
                    return true;
                }
                current = current.Left;
            }
            else
            {
                if (current.Right == null)
                {
                    current.Right = new TreeNode(value);
                    Count++;
                    return true;
                }
                current = current.Right;
            }
        }
    }

    // ---------------- SEARCH ----------------
    public TreeNode? Search(int value)
    {
        TreeNode? current = _root;
        while (current != null)
        {
            if (value == current.Value) return current;
            current = value < current.Value ? current.Left : current.Right;
        }
        return null;
    }

    public bool Contains(int value) => Search(value) != null;

    // ---------------- DELETE ----------------
    /// <returns>true if the value was found and removed.</returns>
    public bool Delete(int value)
    {
        bool deleted = false;
        _root = Delete(_root, value, ref deleted);
        if (deleted) Count--;
        return deleted;
    }

    private static TreeNode? Delete(TreeNode? node, int value, ref bool deleted)
    {
        if (node == null)
            return null;

        if (value < node.Value)
        {
            node.Left = Delete(node.Left, value, ref deleted);
        }
        else if (value > node.Value)
        {
            node.Right = Delete(node.Right, value, ref deleted);
        }
        else
        {
            deleted = true;

            // Case 1 and 2: zero or one child
            if (node.Left == null) return node.Right;
            if (node.Right == null) return node.Left;

            // Case 3: two children -> replace with in-order successor
            TreeNode successor = FindMin(node.Right);
            node.Value = successor.Value;
            bool dummy = false;
            node.Right = Delete(node.Right, successor.Value, ref dummy);
        }
        return node;
    }

    private static TreeNode FindMin(TreeNode node)
    {
        while (node.Left != null)
            node = node.Left;
        return node;
    }

    // ---------------- TRAVERSALS (depth-first) ----------------
    public List<int> PreOrder()
    {
        var result = new List<int>();
        PreOrder(_root, result);
        return result;
    }

    public List<int> InOrder()
    {
        var result = new List<int>();
        InOrder(_root, result);
        return result;
    }

    public List<int> PostOrder()
    {
        var result = new List<int>();
        PostOrder(_root, result);
        return result;
    }

    // Root -> Left -> Right
    private static void PreOrder(TreeNode? node, List<int> result)
    {
        if (node == null) return;
        result.Add(node.Value);
        PreOrder(node.Left, result);
        PreOrder(node.Right, result);
    }

    // Left -> Root -> Right
    private static void InOrder(TreeNode? node, List<int> result)
    {
        if (node == null) return;
        InOrder(node.Left, result);
        result.Add(node.Value);
        InOrder(node.Right, result);
    }

    // Left -> Right -> Root
    private static void PostOrder(TreeNode? node, List<int> result)
    {
        if (node == null) return;
        PostOrder(node.Left, result);
        PostOrder(node.Right, result);
        result.Add(node.Value);
    }

    // ---------------- UTILITIES ----------------
    public int Height() => Height(_root);

    private static int Height(TreeNode? node)
    {
        if (node == null) return 0;
        return 1 + Math.Max(Height(node.Left), Height(node.Right));
    }

    public void Clear()
    {
        _root = null;
        Count = 0;
    }
}
