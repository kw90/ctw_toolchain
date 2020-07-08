using System;
using GurobiModel;
using Gurobi;

namespace BenchmarkTool.Solver.Gurobi
{
    public class GurobiMbSolver : GurobiSolver
    {
       
        public void Generate(string datFilePath)
        {
            GenerateBaseModel(datFilePath);

            try
            {
                int bigM = _k;

                GRBVar[] cableIsStored = new GRBVar[_b]; // indicator whether cable (i,j) requires storage
                GRBVar[,] cableIsStoredAtPosition = new GRBVar[_b, _k]; // indicator whether cable (i,j) requires storage at time t
                GRBVar[,] cableIsPluggedBefore = new GRBVar[_b, _k]; // indicator whether i and j are plugged before t
                GRBVar[,] cableIsPluggedAfter = new GRBVar[_b, _k]; // indicator whether i and j are plugged after t
                GRBVar[] minimum = new GRBVar[_b]; // min{t[i],t[j]}
                GRBVar[] maximum = new GRBVar[_b]; // max{t[i],t[j]}
                for (int i = 0; i < _b; i++)
                {
                    cableIsStored[i] = _model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, null);
                    minimum[i] = _model.AddVar(0.0, _k - 1.0, 0.0, GRB.CONTINUOUS, null);
                    maximum[i] = _model.AddVar(0.0, _k - 1.0, 0.0, GRB.CONTINUOUS, null);
                    for (var j = 0; j < _k; j++)
                    {
                        cableIsStoredAtPosition[i, j] = _model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, null);
                        cableIsPluggedBefore[i, j] = _model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, null);
                        cableIsPluggedAfter[i, j] = _model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, null);
                    }
                }

                var lt = new GRBVar[_k, _k];
                for (var i = 0; i < _k; i++)
                {
                    for (var j = 0; j < _k; j++)
                    {
                        lt[i, j] = _model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, null);
                    }
                }

                // all different
                for (var i = 0; i < _k; i++)
                {
                    for (var j = 0; j < _k; j++)
                    {
                        if (i != j)
                        {
                            _model.AddConstr(pfc[i] - pfc[j] + 1 <= bigM * (1 - lt[i, j]), null);
                            _model.AddConstr(pfc[j] - pfc[i] + 1 <= bigM * lt[i, j], null);
                        }
                    }
                }

                // S
                var expr = new GRBLinExpr();
                for (var i = 0; i < _b; i++)
                {
                    expr.AddTerm(1.0, cableIsStored[i]);
                }

                _model.AddConstr(S == expr, "S");

                // N
                expr = new GRBLinExpr();
                var oPen = new GRBVar[_constraintModel.SoftAtomicConstraints.Count];
                for (var i = 0; i < _constraintModel.SoftAtomicConstraints.Count; i++)
                {
                    oPen[i] = _model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, null);
                    var softAtomicConstraint = _constraintModel.SoftAtomicConstraints[i];
                    _model.AddConstr(pfc[softAtomicConstraint.After - 1] - pfc[softAtomicConstraint.Before - 1] <= bigM * (1 - oPen[i]), null);
                    _model.AddConstr(pfc[softAtomicConstraint.Before - 1] - pfc[softAtomicConstraint.After - 1] <= bigM * oPen[i], null);
                    expr.AddTerm(1.0, oPen[i]);
                }

                _model.AddConstr(N == expr, "N");

                // L
                for (var i = 0; i < _b; i++)
                {
                    _model.AddConstr(maximum[i] - minimum[i] - 1 <= L, "L");

                    // a[i,j]=1 <=> cable (i,j) requires storage
                    _model.AddConstr(2 - maximum[i] + minimum[i] <= bigM * (1 - cableIsStored[i]), "requires_storage1");
                    _model.AddConstr(maximum[i] - minimum[i] - 1 <= bigM * cableIsStored[i], "requires_storage2");
                }

                // b[i,j,t]=1 <=> cable (i,j) requires storage at time t
                for (var p = 0; p < _b; p++)
                {
                    for (var ti = 0; ti < _k; ti++)
                    {
                        _model.AddConstr(minimum[p] - ti + 1 <= bigM * (1 - cableIsStoredAtPosition[p, ti]),
                            "requires_storage_at_time_t1");
                        _model.AddConstr(ti - maximum[p] + 1 <= bigM * (1 - cableIsStoredAtPosition[p, ti]),
                            "requires_storage_at_time_t2");
                        _model.AddConstr(ti - minimum[p] <= bigM * (1 - cableIsPluggedBefore[p, ti]),
                            "requires_storage_at_time3");
                        _model.AddConstr(maximum[p] - ti <= bigM * (1 - cableIsPluggedAfter[p, ti]),
                            "requires_storage_at_time_t4");
                        _model.AddConstr(
                            cableIsStoredAtPosition[p, ti] + cableIsPluggedBefore[p, ti] + cableIsPluggedAfter[p, ti] ==
                            1, "requires_storage_at_time_t5");
                    }

                    // min/max constraints
                    _model.AddConstr(minimum[p] - pfc[p] <= 0, "is_lower_bound1");
                    _model.AddConstr(minimum[p] - pfc[p + _b] <= 0, "is_lower_bound2");
                    _model.AddConstr(pfc[p] - minimum[p] <= bigM * (1 - lt[p, p + _b]),
                        "minimum_is_first");
                    _model.AddConstr(pfc[p + _b] - minimum[p] <= bigM * lt[p, p + _b],
                        "minimum_is_last");
                    _model.AddConstr(maximum[p] == pfc[p] + pfc[p + _b] - minimum[p], "set_maximum");
                }

                // M
                for (var i = 0; i < _k; i++)
                {
                    var expr2 = new GRBLinExpr();
                    for (var j = 0; j < _b; j++)
                    {
                        expr2.AddTerm(1.0, cableIsStoredAtPosition[j, i]);
                    }

                    _model.AddConstr(expr2 <= M, "M");
                }

                // Atomic constraints
                foreach (var constraint in _constraintModel.AtomicConstraints)
                {
                    int ordSmaller = constraint.Before;
                    int ordLarger = constraint.After;
                    if (ordSmaller < ordLarger)
                    {
                        _model.AddConstr(lt[constraint.Before - 1, constraint.After - 1] == 1, null);
                    }
                    else
                    {
                        _model.AddConstr(lt[constraint.After - 1, constraint.Before - 1] == 0, null);
                    }
                }

                // Disjunctive constraints
                foreach (var constraint in _constraintModel.DisjunctiveConstraints)
                {
                    int ordFirstSmaller = constraint.Disjunct1.Before;
                    int ordFirstLarger = constraint.Disjunct1.After;
                    int ordLastSmaller = constraint.Disjunct2.Before;
                    int ordLastLarger = constraint.Disjunct2.After;

                    if (ordFirstSmaller < ordFirstLarger && ordLastSmaller < ordLastLarger)
                    {
                        _model.AddConstr(
                            lt[constraint.Disjunct1.Before - 1, constraint.Disjunct1.After - 1] +
                            lt[constraint.Disjunct2.Before - 1, constraint.Disjunct2.After - 1] >= 1, null);
                    }

                    if (ordFirstSmaller < ordFirstLarger && ordLastSmaller > ordLastLarger)
                    {
                        _model.AddConstr(
                            lt[constraint.Disjunct1.Before - 1, constraint.Disjunct1.After - 1] + 1 -
                            lt[constraint.Disjunct2.After - 1, constraint.Disjunct2.Before - 1] >= 1, null);
                    }

                    if (ordFirstSmaller > ordFirstLarger && ordLastSmaller < ordLastLarger)
                    {
                        _model.AddConstr(1 -
                                         lt[constraint.Disjunct1.After - 1, constraint.Disjunct1.Before - 1] +
                                         lt[constraint.Disjunct2.Before - 1, constraint.Disjunct2.After - 1] >= 1,
                            null);
                    }

                    if (ordFirstSmaller > ordFirstLarger && ordLastSmaller > ordLastLarger)
                    {
                        _model.AddConstr(1 -
                                         lt[constraint.Disjunct1.After - 1, constraint.Disjunct1.Before - 1] + 1 -
                                         lt[constraint.Disjunct2.After - 1, constraint.Disjunct2.Before - 1] >= 1,
                            null);
                    }
                }

                // direct successor constraint
                foreach (int chamber in _constraintModel.DirectSuccessors)
                {
                    //           if (chamber + _b < _b * 2)
                    //         {
                    if (chamber <= _b)
                    {
                        _model.AddConstr(pfc[chamber + _b - 1] - pfc[chamber - 1] - 1 <= bigM * (1 - lt[chamber - 1, chamber + _b - 1]), null);
                    }
                    else
                    {
                        _model.AddConstr(pfc[chamber - 1] - pfc[chamber - _b - 1] - 1 <= bigM * lt[chamber - 1, chamber - _b - 1], null);
                    }
                    //       }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
