using System.Collections;

namespace Sakuga;

public abstract partial class Element
{
    public abstract string Draw();

    public static implicit operator Element(string s)
    {
        return new StringElement(s);
    }

    public static implicit operator Element(List<string> strings)
    {
        return new StringListElement(strings);
    }

    public static implicit operator Element(string[] strings)
    {
        return new StringListElement(strings);
    }

    public static implicit operator Element(List<Element> elements)
    {
        return new ElementListElement(elements);
    }

    public static implicit operator Element(Element[] elements)
    {
        return new ElementListElement(elements);
    }
}

public class StringElement(string element): Element
{
    public override string Draw()
    {
        return element;
    }
}

public class StringListElement(ICollection<string> strings) : Element
{
    public override string Draw()
    {
        return string.Join("", strings);
    }
}

public class ElementListElement(ICollection<Element> elements) : Element
{
    public override string Draw()
    {
        return string.Join("", elements.Select(e => e.Draw()));
    }
}

public class LineStack : Element, IList<Element>
{
    private readonly List<Element> _inner = [];

    public override string Draw()
    {
        return string.Join(Environment.NewLine, _inner.Select(e => e.Draw()));
    }

    public IEnumerator<Element> GetEnumerator()
    {
        return _inner.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(Element item)
    {
        _inner.Add(item);
    }

    public void Clear()
    {
        _inner.Clear();
    }

    public bool Contains(Element item)
    {
        return _inner.Contains(item);
    }

    public void CopyTo(Element[] array, int arrayIndex)
    {
        _inner.CopyTo(array, arrayIndex);
    }

    public bool Remove(Element item)
    {
        return _inner.Remove(item);
    }

    public int Count => _inner.Count;
    public bool IsReadOnly => false;

    public int IndexOf(Element item)
    {
        return _inner.IndexOf(item);
    }

    public void Insert(int index, Element item)
    {
        _inner.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        _inner.RemoveAt(index);
    }

    public Element this[int index]
    {
        get => _inner[index];
        set => _inner[index] = value;
    }

    public void AddRange(IEnumerable<Element> collection)
    {
        _inner.AddRange(collection);
    }
}