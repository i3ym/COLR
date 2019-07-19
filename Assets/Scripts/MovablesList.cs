using System.Collections;
using System.Collections.Generic;

public class MovablesList : IEnumerable<Movable>, IList<Movable>
{
    List<Movable> Movables = new List<Movable>();
    public List<Meteor> Meteors = new List<Meteor>();
    public List<Bullet> Bullets = new List<Bullet>();

    public Movable this [int index]
    {
        get => Movables[index];
        set => Movables[index] = value;
    }

    public int Count => Movables.Count;

    public bool IsReadOnly => false;

    public void Add(Movable item)
    {
        Movables.Add(item);

        if (item is Meteor) Meteors.Add(item as Meteor);
        else if (item is Bullet) Bullets.Add(item as Bullet);
    }

    public void Clear()
    {
        Movables.Clear();
        Meteors.Clear();
        Bullets.Clear();
    }

    public bool Contains(Movable item) => Movables.Contains(item);

    public void CopyTo(Movable[] array, int arrayIndex) => Movables.CopyTo(array, arrayIndex);

    public IEnumerator<Movable> GetEnumerator() => Movables.GetEnumerator();

    public int IndexOf(Movable item) => Movables.IndexOf(item);

    public void Insert(int index, Movable item) => Movables.Insert(index, item);

    public bool Remove(Movable item)
    {
        if (!Movables.Remove(item)) return false;

        if (item is Meteor) Meteors.Remove(item as Meteor);
        else if (item is Bullet) Bullets.Remove(item as Bullet);

        return true;
    }

    public void RemoveAt(int index) => Remove(Movables[index]);

    IEnumerator IEnumerable.GetEnumerator() => Movables.GetEnumerator();
}