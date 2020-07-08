
using System;
using System.IO;
using System.Linq;
using GurobiModel;
using Gurobi;

namespace BenchmarkTool.Solver.Gurobi
{
    public sealed class GurobiMdSolver : GurobiSolver
    {
        public void Generate(string datFilePath)
        {
            GenerateBaseModel(datFilePath);

            try
            {
                var cfp = CreatePermutationVariable();

                // all different
                for (var i = 0; i < _k; i++)
                {
                    for (var j = 0; j < _k; j++)
                    {
                        if (i != j)
                        {
                            var bin = _model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, "binaryVar_pfc");
                            _model.AddConstr(pfc[i] - pfc[j] <= -1 + (_k * bin), null);
                            _model.AddConstr(pfc[j] - pfc[i] <= -1 + (_k * (1 - bin)), null);

                            var bin2 = _model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, "binaryVar_cfp");
                            _model.AddConstr(cfp[i] - cfp[j] <= -1 + (_k * bin2), null);
                            _model.AddConstr(cfp[j] - cfp[i] <= -1 + (_k * (1 - bin2)), null);
                        }
                    }
                }

                // channeling constraint pfc-cfp
                for (var j = 0; j < _k; j++)
                {
                    for (var p = 0; p < _k; p++)
                    {
                        var pSmallerThanPfc = _model.AddVar(0, 1, 0, GRB.BINARY, null);
                        var pBiggerThanPfc = _model.AddVar(0, 1, 0, GRB.BINARY, null);
                        var equalsP = _model.AddVar(0, 1, 0, GRB.BINARY, null);

                        _model.AddConstr(pfc[j] - p <= _k * pSmallerThanPfc, null);
                        _model.AddConstr(p - pfc[j] <= _k * pBiggerThanPfc, null);

                        _model.AddConstr(pfc[j] - p + 1 <= _k * (1 - pBiggerThanPfc), null);
                        _model.AddConstr(p - pfc[j] + 1 <= _k * (1 - pSmallerThanPfc), null);


                        _model.AddConstr(pBiggerThanPfc + pSmallerThanPfc <= 1, null);
                        _model.AddConstr(equalsP + pBiggerThanPfc + pSmallerThanPfc == 1, null);

                        _model.AddGenConstrIndicator(equalsP, 1, cfp[p], GRB.EQUAL, j, null);
                    }
                }

                // the following is the same as the Mi model
                // atomic constraints
                foreach (var constraint in _constraintModel.AtomicConstraints)
                {
                    _model.AddConstr(pfc[constraint.Before - 1] - pfc[constraint.After - 1] <= 0, null);
                }

                // disjunctive constraints
                foreach (var constraint in _constraintModel.DisjunctiveConstraints)
                {
                    var bin = _model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, null);
                    _model.AddConstr(pfc[constraint.Disjunct1.Before - 1] - pfc[constraint.Disjunct1.After - 1] <= _k * bin, null);
                    _model.AddConstr(pfc[constraint.Disjunct2.Before - 1] - pfc[constraint.Disjunct2.After - 1] <= _k * (1 - bin), null);
                }

                // direct successor
                foreach (int i in _constraintModel.DirectSuccessors.Select(x => x - 1))
                {
                    if (i < _b)
                    {
                        var bin = _model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, null);
                        _model.AddConstr(pfc[i] - pfc[i + _b] <= _k * bin, null);
                        _model.AddConstr(pfc[i + _b] - pfc[i] <= _k * (1 - bin), null);
                        _model.AddConstr(pfc[i + _b] - pfc[i] - 1 <= _k * bin, null);
                    }
                    else
                    {
                        var bin = _model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, null);
                        _model.AddConstr(pfc[i] - pfc[i - _b] <= _k * bin, null);
                        _model.AddConstr(pfc[i - _b] - pfc[i] <= _k * (1 - bin), null);
                        _model.AddConstr(pfc[i - _b] - pfc[i] - 1 <= _k * bin, null);
                    }
                }

                // helper definitions for the calculation of the optimization criteria
                // determine which end comes first in the permutation
                var secondEndOfCable = new GRBVar[_b];
                var firstEndOfCable = new GRBVar[_b];
                for (var i = 0; i < _b; i++)
                {
                    secondEndOfCable[i] = _model.AddVar(0.0, _k, 0.0, GRB.INTEGER, null);
                    firstEndOfCable[i] = _model.AddVar(0.0, _k, 0.0, GRB.INTEGER, null);
                    _model.AddGenConstrMax(secondEndOfCable[i], new[] { pfc[i], pfc[i + _b] }, 0.0, null);
                    _model.AddConstr(firstEndOfCable[i] == pfc[i] + pfc[i + _b] - secondEndOfCable[i], null);
                }

                // determine how long a cable is put into storage
                var storageTimeForCable = new GRBVar[_b];
                for (var i = 0; i < _b; i++)
                {
                    storageTimeForCable[i] = _model.AddVar(0, _k - 1, 0.0, GRB.INTEGER, null);
                    _model.AddConstr(storageTimeForCable[i] == secondEndOfCable[i] - firstEndOfCable[i] - 1, null);
                }

                // S
                var sLinExpr = new GRBLinExpr();
                var isInterrupted = new GRBVar[_b];
                for (var i = 0; i < _b; i++)
                {
                    isInterrupted[i] = _model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, null);
                    _model.AddConstr(secondEndOfCable[i] - firstEndOfCable[i] <= 1 + (_k - 1) * isInterrupted[i], null);
                    _model.AddConstr(secondEndOfCable[i] - firstEndOfCable[i] >= 2 * isInterrupted[i], null);
                    sLinExpr.AddTerm(1.0, isInterrupted[i]);
                }
                _model.AddConstr(S == sLinExpr, "S");

                // M (maximum of simultaneously stored cables)

                var cableIsStoredAtStep = new GRBVar[_b, _k]; // indicator whether cable requires storage at step t
                var cableIsPluggedBefore = new GRBVar[_b, _k]; // indicator whether cable are plugged before step t
                var cableIsPluggedAfter = new GRBVar[_b, _k]; // indicator whether cable are plugged after step t
                for (var i = 0; i < _b; i++)
                {
                    for (var step = 0; step < _k; step++)
                    {
                        cableIsStoredAtStep[i, step] = _model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, null);
                        cableIsPluggedBefore[i, step] = _model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, null);
                        cableIsPluggedAfter[i, step] = _model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, null);

                        _model.AddConstr(firstEndOfCable[i] - step + 1 <= _k * (1 - cableIsStoredAtStep[i, step]), null);
                        _model.AddConstr(step - secondEndOfCable[i] + 1 <= _k * (1 - cableIsStoredAtStep[i, step]), null);
                        _model.AddConstr(step - firstEndOfCable[i] <= _k * (1 - cableIsPluggedBefore[i, step]), null);
                        _model.AddConstr(secondEndOfCable[i] - step <= _k * (1 - cableIsPluggedAfter[i, step]), null);
                        _model.AddConstr(
                            cableIsStoredAtStep[i, step] + cableIsPluggedBefore[i, step] + cableIsPluggedAfter[i, step] == 1, null);
                    }
                }

                for (var i = 0; i < _k; i++)
                {
                    var mLinExpr = new GRBLinExpr();
                    for (var j = 0; j < _b; j++)
                    {
                        mLinExpr.AddTerm(1.0, cableIsStoredAtStep[j, i]);
                    }

                    _model.AddConstr(mLinExpr <= M, "M");
                }

                // L (longest time cable is in storage)
                _model.AddGenConstrMax(L, storageTimeForCable, 0.0, null);

                // N (number of violated soft constraints)
                var nLinExpr = new GRBLinExpr();
                var softConstraintIsViolated = new GRBVar[_constraintModel.SoftAtomicConstraints.Count];
                for (var i = 0; i < _constraintModel.SoftAtomicConstraints.Count; i++)
                {
                    softConstraintIsViolated[i] = _model.AddVar(0.0, 1.0, 0.0, GRB.BINARY, null);
                    var softAtomicConstraint = _constraintModel.SoftAtomicConstraints[i];
                    _model.AddConstr(pfc[softAtomicConstraint.After - 1] - pfc[softAtomicConstraint.Before - 1] <= _k * (1 - softConstraintIsViolated[i]), null);
                    _model.AddConstr(pfc[softAtomicConstraint.Before - 1] - pfc[softAtomicConstraint.After - 1] <= _k * softConstraintIsViolated[i], null);
                    nLinExpr.AddTerm(1.0, softConstraintIsViolated[i]);
                }

                _model.AddConstr(N == nLinExpr, "N");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
