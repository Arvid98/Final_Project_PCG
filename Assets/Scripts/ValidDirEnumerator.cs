using System.Collections;
using System.Collections.Generic;

public struct ValidDirEnumerator : IEnumerable<Point>, IEnumerator<Point>
{
    private int _state;
    private readonly int _x;
    private readonly int _y;
    private readonly int _w;
    private readonly int _h;

    public ValidDirEnumerator(int x, int y, int width, int height)
    {
        _x = x;
        _y = y;
        _w = width;
        _h = height;
        _state = 0;
    }

    public readonly Point Current => _state switch
    {
        1 => new Point(-1, 0),
        2 => new Point(0, -1),
        3 => new Point(1, 0),
        4 => new Point(0, 1),
        _ => new Point(),
    };

    readonly object IEnumerator.Current => Current;

    public readonly void Dispose()
    {
    }

    public readonly ValidDirEnumerator GetEnumerator() => this;

    public bool MoveNext()
    {
        while (_state < 4)
        {
            _state++;
            switch (_state)
            {
                case 1 when _x > 0:
                case 2 when _y > 0:
                case 3 when _x < _w - 1:
                case 4 when _y < _h - 1:
                    return true;
            }
        }
        return false;
    }

    public readonly void Reset()
    {
    }

    readonly IEnumerator<Point> IEnumerable<Point>.GetEnumerator() => GetEnumerator();

    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}