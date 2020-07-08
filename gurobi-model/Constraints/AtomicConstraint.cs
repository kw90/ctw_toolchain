namespace GurobiModel.Constraints
{
    public class AtomicConstraint
    {
        public AtomicConstraint(int before, int after)
        {
            Before = before;
            After = after;
        }

        public int Before { get; set; }
        public int After { get; set; }

        public override string ToString()
        {
            return Before + " < " + After;
        }
    }
}
