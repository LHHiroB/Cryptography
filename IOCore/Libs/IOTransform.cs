using System;
using System.Collections.Generic;
using System.Linq;

namespace IOCore.Libs
{
    public class IOTransform
    {
        public enum Type
        {
            None,
            Rotate90,
            Rotate180,
            Rotate270,
            Flop,
            Flip,
        };

        public static readonly Dictionary<Type, string> TRANSFORMS = new() {
            { Type.None, null },
            { Type.Rotate90, "r" },
            { Type.Rotate180, "rr" },
            { Type.Rotate270, "rrr" },
            { Type.Flop, "|" },
            { Type.Flip, "rr|" },
        };

        private string _steps = string.Empty;

        public Type[] Steps
        {
            get
            {
                _steps ??= string.Empty;

                var transforms = _steps;
                var run = true;

                while (run)
                {
                    run = false;

                    while (transforms.Contains("rrr"))
                    {
                        run = true;
                        transforms = transforms.Replace("rrr", "#");
                    }

                    while (transforms.Contains("rr"))
                    {
                        run = true;
                        transforms = transforms.Replace("rr", "+");
                    }
                }

                var steps = new Type[transforms.Length];

                for (int i = 0; i < transforms.Length; i++)
                {
                    steps[i] = transforms[i] switch
                    {
                        'r' => Type.Rotate90,
                        '+' => Type.Rotate180,
                        '#' => Type.Rotate270,
                        '|' => Type.Flop,
                        _ => Type.None
                    };
                }

                return steps;
            }
        }

        public void Add(Type step)
        {
            _steps ??= string.Empty;

            var transforms = _steps;
            transforms += TRANSFORMS[step];

            var run = true;

            while (run)
            {
                run = false;

                while (transforms.Contains("rrrr"))
                {
                    run = true;
                    transforms = transforms.Replace("rrrr", "");
                }

                while (transforms.Contains("||"))
                {
                    run = true;
                    transforms = transforms.Replace("||", "");
                }

                while (transforms.Contains("rrr|"))
                {
                    run = true;
                    transforms = transforms.Replace("rrr|", "|r");
                }
            }

            _steps = transforms;
        }

        public void Empty() => _steps = string.Empty;
        public int Count(Func<char, bool> pre) => _steps?.Count(pre) ?? 0;
    }
}
