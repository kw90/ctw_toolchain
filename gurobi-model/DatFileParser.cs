using System;
using System.IO;
using System.Linq;
using GurobiModel.Constraints;

namespace GurobiModel
{

    public static class DatFileParser
    {
        private enum ParseState
        {
            None,
            AtomicConstraints,
            SoftAtomicConstraints,
            DisjunctiveConstraints,
            DirectSuccessors
        }

        public static ConstraintModel ParseDatFile(string path)
        {
            if (!path.EndsWith(".dat"))
            {
                var fileWithoutExtension = path;
                while (fileWithoutExtension.Contains('.'))
                {
                    fileWithoutExtension = Path.GetFileNameWithoutExtension(fileWithoutExtension);
                }

                path = Path.GetDirectoryName(path) + @"\" + fileWithoutExtension + ".dat";
            }

            var model = new ConstraintModel();
            var state = ParseState.None;

            foreach (var line in File.ReadAllLines(path).Select(line => line.Trim()))
            {
                if (line.Contains("=") && line.EndsWith(";"))
                {
                    if (line.StartsWith("k"))
                    {
                        model.Nbchambers = ParseInt(line);
                    }

                    if (line.StartsWith("b"))
                    {
                        model.B = ParseInt(line);
                    }
                }

                if (line.EndsWith("};"))
                {
                    state = ParseState.None;
                }

                if (state != ParseState.None)
                {
                    var ints = ParseTuple(line);
                    if (state == ParseState.AtomicConstraints)
                    {
                        model.AtomicConstraints.Add(ParseAtomicConstraint(ints));
                    }

                    if (state == ParseState.SoftAtomicConstraints)
                    {
                        model.SoftAtomicConstraints.Add(ParseAtomicConstraint(ints));
                    }

                    if (state == ParseState.DisjunctiveConstraints)
                    {
                        model.DisjunctiveConstraints.Add(ParseDisjunctiveConstraint(ints));
                    }

                    if (state == ParseState.DirectSuccessors)
                    {
                        model.DirectSuccessors.Add(ParseDirectSuccessor(ints));
                    }
                }

                if (line.StartsWith("AtomicConstraints = {"))
                {
                    state = ParseState.AtomicConstraints;
                }

                if (line.StartsWith("SoftAtomicConstraints = {"))
                {
                    state = ParseState.SoftAtomicConstraints;
                }

                if (line.StartsWith("DisjunctiveConstraints = {"))
                {
                    state = ParseState.DisjunctiveConstraints;
                }

                if (line.StartsWith("DirectSuccessors = {"))
                {
                    state = ParseState.DirectSuccessors;
                }
            }

            return model;
        }

        private static int ParseDirectSuccessor(int[] ints)
        {
            if (ints.Length != 1)
            {
                throw new ArgumentException("Failed to parse Direct Successors!", nameof(ints));
            }

            return ints[0];
        }

        private static DisjunctiveConstraint ParseDisjunctiveConstraint(int[] ints)
        {
            if (ints.Length != 4)
            {
                throw new ArgumentException("Failed to parse Disjunctive Constraint!", nameof(ints));
            }

            var disjunct1 = new AtomicConstraint(ints[0], ints[1]);
            var disjunct2 = new AtomicConstraint(ints[2], ints[3]);
            return new DisjunctiveConstraint(disjunct1, disjunct2);
        }

        private static AtomicConstraint ParseAtomicConstraint(int[] ints)
        {
            if (ints.Length != 2)
            {
                throw new ArgumentException("Failed to parse Atomic Constraint!", nameof(ints));
            }

            return new AtomicConstraint(ints[0], ints[1]);
        }

        private static int[] ParseTuple(string line)
        {
            return line.Replace("<", string.Empty)
                .Replace(">,", string.Empty)
                .Replace(">", string.Empty)
                .Replace(" ", string.Empty)
                .Split(',')
                .Where(x => !string.IsNullOrEmpty(x.Trim()))
                .Select(int.Parse).ToArray();
        }

        private static int ParseInt(string line)
        {
            var splits = line.Split('=').Select(split => split.Replace(";", string.Empty).Trim()).ToArray();
            if (splits.Count() != 2)
            {
                throw new ArgumentException("Data file has wrong format. Failed to parse x = y;", nameof(line));
            }

            return int.Parse(splits[1]);
        }
    }
}
