using System.Text;

namespace Ipfs;

/// <summary>
///  Tests for <see cref="DagNode"/>
/// </summary>
[TestClass]
public class DagNodeTest
{
    /// <summary>
    ///  Add a link to a DAG node.
    /// </summary>
    [TestMethod]
    public void AddLink()
    {
        byte[] a = Encoding.UTF8.GetBytes("a");
        var anode = new DagNode(a);

        byte[] b = Encoding.UTF8.GetBytes("b");
        var bnode = new DagNode(b);

        DagNode cnode = bnode.AddLink(anode.ToLink());
        Assert.IsFalse(ReferenceEquals(bnode, cnode));
        Assert.AreEqual(1, cnode.DataBytes.Length);
        Assert.HasCount(1, cnode.ReadOnlyLinks);
        Assert.AreEqual(anode.Id, cnode.ReadOnlyLinks.First().Id);
        Assert.AreEqual(anode.Size, cnode.ReadOnlyLinks.First().Size);

        RoundtripTest(cnode);
    }

    /// <summary>
    /// A DAG node with only data.
    /// </summary>
    [TestMethod]
    public void DataOnlyDAG()
    {
        byte[] abc = Encoding.UTF8.GetBytes("abc");
        var node = new DagNode(abc);
        CollectionAssert.AreEqual(abc, node.DataBytes.ToArray());
        Assert.IsEmpty(node.ReadOnlyLinks);
        Assert.AreEqual("QmXg9Pp2ytZ14xgmQjYEiHjVjMFXzCVVEcRTWJBmLgR39V", (string)node.Id);
        Assert.AreEqual<ulong>(5, node.Size);

        RoundtripTest(node);
    }

    /// <summary>
    /// An empty DAG node.
    /// </summary>ReadOnly
    [TestMethod]
    public void EmptyDAG()
    {
        var node = new DagNode((byte[]?)null);
        Assert.AreEqual(0, node.DataBytes.Length);
        Assert.IsEmpty(node.ReadOnlyLinks);
        Assert.AreEqual<ulong>(0, node.Size);
        Assert.AreEqual("QmdfTbBqBPQ7VNxZEYEj14VmRuZBkqFbiwReogJgS1zR1n", (string)node.Id);

        RoundtripTest(node);
    }

    /// <summary>
    /// A DAG node with data and a non-default hashing algorithm.
    /// </summary>
    [TestMethod]
    public void Hashing_Algorithm()
    {
        byte[] abc = Encoding.UTF8.GetBytes("abc");
        var node = new DagNode(abc, null, "sha2-512");
        CollectionAssert.AreEqual(abc, node.DataBytes.ToArray());
        Assert.IsEmpty(node.ReadOnlyLinks);
        Assert.AreEqual("bafybgqdqrys7323fuivxoixir7nnsfqmsneuuseg6mkbmcjgj4xaq7suehcmbghv5sbtxu57ccnhqjggxx7iz5p77gkcrhv2i3pj3yhv7fi56", (string)node.Id);
        Assert.AreEqual<ulong>(5, node.Size);

        RoundtripTest(node);
    }

    /// <summary>
    /// A DAG node with a link using CIDv1.
    /// </summary>
    [TestMethod]
    public void Link_With_CID_V1()
    {
        byte[] data = "124F0A4401551340309ECC489C12D6EB4CC40F50C902F2B4D0ED77EE511A7C7A9BCD3CA86D4CD86F989DD35BC5FF499670DA34255B45B0CFD830E81F605DCF7DC5542E93AE9CD76F120568656C6C6F180B0A020801"
            .ToHexBuffer();
        var ms = new MemoryStream(data, false);
        var node = new DagNode(ms);
        Assert.AreEqual("0801", node.DataBytes.ToArray().ToHexString());
        Assert.HasCount(1, node.ReadOnlyLinks);
        IMerkleLink link = node.ReadOnlyLinks.First();
        Assert.AreEqual("hello", link.Name);
        Assert.AreEqual(1, link.Id.Version);
        Assert.AreEqual("raw", link.Id.ContentType);
        Assert.AreEqual("sha2-512", link.Id.Hash.Algorithm.Name);
        Assert.AreEqual<ulong>(11, link.Size);
    }

    /// <summary>
    /// A DAG node with only a link.
    /// </summary>
    [TestMethod]
    public void LinkOnlyDAG()
    {
        byte[] a = Encoding.UTF8.GetBytes("a");
        var anode = new DagNode(a);
        IMerkleLink alink = anode.ToLink("a");

        var node = new DagNode(null, [alink]);
        Assert.AreEqual(0, node.DataBytes.Length);
        Assert.HasCount(1, node.ReadOnlyLinks);
        Assert.AreEqual("QmVdMJFGTqF2ghySAmivGiQvsr9ZH7ujnNGBkLNNCe4HUE", (string)node.Id);
        Assert.AreEqual<ulong>(43, node.Size);

        RoundtripTest(node);
    }

    /// <summary>
    /// Links are sorted by name.
    /// </summary>
    [TestMethod]
    public void Links_are_Sorted()
    {
        byte[] a = Encoding.UTF8.GetBytes("a");
        var anode = new DagNode(a);
        IMerkleLink alink = anode.ToLink("a");

        byte[] b = Encoding.UTF8.GetBytes("b");
        var bnode = new DagNode(b);
        IMerkleLink blink = bnode.ToLink("b");

        byte[] ab = Encoding.UTF8.GetBytes("ab");
        var node = new DagNode(ab, [blink, alink]);
        CollectionAssert.AreEqual(ab, node.DataBytes.ToArray());
        Assert.HasCount(2, node.ReadOnlyLinks);
        Assert.AreEqual("Qma5sYpEc9hSYdkuXpMDJYem95Mj7hbEd9C412dEQ4ZkfP", (string)node.Id);
    }

    /// <summary>
    /// A DAG node with data and multiple links.
    /// </summary>
    [TestMethod]
    public void MultipleLinksDataDAG()
    {
        byte[] a = Encoding.UTF8.GetBytes("a");
        var anode = new DagNode(a);
        IMerkleLink alink = anode.ToLink("a");

        byte[] b = Encoding.UTF8.GetBytes("b");
        var bnode = new DagNode(b);
        IMerkleLink blink = bnode.ToLink("b");

        byte[] ab = Encoding.UTF8.GetBytes("ab");
        var node = new DagNode(ab, [alink, blink]);
        CollectionAssert.AreEqual(ab, node.DataBytes.ToArray());
        Assert.HasCount(2, node.ReadOnlyLinks);
        Assert.AreEqual("Qma5sYpEc9hSYdkuXpMDJYem95Mj7hbEd9C412dEQ4ZkfP", (string)node.Id);

        RoundtripTest(node);
    }

    /// <summary>
    /// A DAG node with multiple links and no data.
    /// </summary>
    [TestMethod]
    public void MultipleLinksOnlyDAG()
    {
        byte[] a = Encoding.UTF8.GetBytes("a");
        var anode = new DagNode(a);
        IMerkleLink alink = anode.ToLink("a");

        byte[] b = Encoding.UTF8.GetBytes("b");
        var bnode = new DagNode(b);
        IMerkleLink blink = bnode.ToLink("b");

        var node = new DagNode(null, [alink, blink]);
        Assert.AreEqual(0, node.DataBytes.Length);
        Assert.HasCount(2, node.ReadOnlyLinks);
        Assert.AreEqual("QmbNgNPPykP4YTuAeSa3DsnBJWLVxccrqLUZDPNQfizGKs", (string)node.Id);

        RoundtripTest(node);
    }

    /// <summary>
    /// Remove a link from a DAG node.
    /// </summary>
    [TestMethod]
    public void RemoveLink()
    {
        byte[] a = Encoding.UTF8.GetBytes("a");
        var anode = new DagNode(a);

        byte[] b = Encoding.UTF8.GetBytes("b");
        var bnode = new DagNode(b);

        byte[] c = Encoding.UTF8.GetBytes("c");
        var cnode = new DagNode(c, [anode.ToLink(), bnode.ToLink()]);

        DagNode dnode = cnode.RemoveLink(anode.ToLink());
        Assert.IsFalse(ReferenceEquals(dnode, cnode));
        Assert.AreEqual(1, dnode.DataBytes.Length);
        Assert.HasCount(1, dnode.ReadOnlyLinks);
        Assert.AreEqual(bnode.Id, dnode.ReadOnlyLinks.First().Id);
        Assert.AreEqual(bnode.Size, dnode.ReadOnlyLinks.First().Size);

        RoundtripTest(cnode);
    }

    /// <summary>
    /// Set the Id of a DAG node.
    /// </summary>
    [TestMethod]
    public void Setting_Id()
    {
        var a = new DagNode((byte[]?)null);
        var b = new DagNode((byte[]?)null)
        {
            // Wrong hash but allowed.
            Id = "QmdfTbBqBPQ7VNxZEYEj14VmRuZBkqFbiwReogJgS1zR1m"
        };
        Assert.AreEqual(a.DataBytes.Length, b.DataBytes.Length);
        Assert.HasCount(a.ReadOnlyLinks.Count, b.ReadOnlyLinks);
        Assert.AreEqual(a.Size, b.Size);
        Assert.AreNotEqual(a.Id, b.Id);

        RoundtripTest(b);
    }

    /// <summary>
    /// Make a link from a DAG node.
    /// </summary>
    [TestMethod]
    public void ToLink()
    {
        byte[] abc = Encoding.UTF8.GetBytes("abc");
        var node = new DagNode(abc);
        IMerkleLink link = node.ToLink();
        Assert.AreEqual("", link.Name);
        Assert.AreEqual(node.Id, link.Id);
        Assert.AreEqual(node.Size, link.Size);
    }

    /// <summary>
    /// Make a named link from a DAG node.
    /// </summary>
    [TestMethod]
    public void ToLink_With_Name()
    {
        byte[] abc = Encoding.UTF8.GetBytes("abc");
        var node = new DagNode(abc);
        IMerkleLink link = node.ToLink("abc");
        Assert.AreEqual("abc", link.Name);
        Assert.AreEqual(node.Id, link.Id);
        Assert.AreEqual(node.Size, link.Size);
    }

    private static void RoundtripTest(DagNode a)
    {
        var ms = new MemoryStream();
        a.Write(ms);
        ms.Position = 0;
        var b = new DagNode(ms);
        CollectionAssert.AreEqual(a.DataBytes.ToArray(), b.DataBytes.ToArray());
        CollectionAssert.AreEqual(a.ToArray(), b.ToArray());
        Assert.HasCount(a.ReadOnlyLinks.Count, b.ReadOnlyLinks);
        _ = a.ReadOnlyLinks.Zip(b.ReadOnlyLinks, (first, second) =>
        {
            Assert.AreEqual(first.Id, second.Id);
            Assert.AreEqual(first.Name, second.Name);
            Assert.AreEqual(first.Size, second.Size);
            return first;
        }).ToArray();

        using (Stream first = a.DataStream)
        using (Stream second = b.DataStream)
        {
            Assert.AreEqual(first.Length, second.Length);
            for (int i = 0; i < first.Length; ++i)
            {
                Assert.AreEqual(first.ReadByte(), second.ReadByte());
            }
        }
    }
}
