using System.Collections.Generic;
using GurobiModel.Constraints;

namespace GurobiModel
    {
        public class ConstraintModel
        {
            public int Nbchambers { get; set; }
            public int B { get; set; }
            public List<AtomicConstraint> AtomicConstraints { get; } = new List<AtomicConstraint>();
            public List<AtomicConstraint> SoftAtomicConstraints { get; } = new List<AtomicConstraint>();
            public List<DisjunctiveConstraint> DisjunctiveConstraints { get; } = new List<DisjunctiveConstraint>();
            public HashSet<int> DirectSuccessors { get; } = new HashSet<int>();
        }
    }

}
}
