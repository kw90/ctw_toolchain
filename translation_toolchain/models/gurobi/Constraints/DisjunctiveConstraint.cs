namespace GurobiModel.Constraints
{
    public class DisjunctiveConstraint
    {
        public DisjunctiveConstraint(AtomicConstraint disjunct1, AtomicConstraint disjunct2)
        {
            Disjunct1 = disjunct1;
            Disjunct2 = disjunct2;
        }

        public AtomicConstraint Disjunct1 { get; set; }

        public AtomicConstraint Disjunct2 { get; set; }

        public override string ToString()
        {
            return Disjunct1 + " OR " + Disjunct2;
        }
    }
}