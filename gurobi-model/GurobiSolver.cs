
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gurobi;

namespace GurobiModel
{
    public class GurobiSolver
    {

        protected GRBModel _model;
        protected ConstraintModel _constraintModel;
        protected GRBVar S;
        protected GRBVar L;
        protected GRBVar M;
        protected GRBVar N;
        protected GRBVar[] pfc;
        protected GRBEnv env;

        public string ExampleName { get; set; }
        protected int _k;
        protected int _b;



        public void GenerateBaseModel(string datFilePath)
        {
            _constraintModel = DatFileParser.ParseDatFile(datFilePath);
            ExampleName = new FileInfo(datFilePath).Name;

            try
            {
                var path = @"C:\IJCAI\Output\TestRuns\Logs\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                env = new GRBEnv(path + ExampleName + ".log")
                {
                    LogToConsole = 0,
                    NodefileStart = 0.5
                };
                _model = new GRBModel(env);

                _k = _constraintModel.K;
                _b = _constraintModel.B;

                // Optimization values
                S = _model.AddVar(0.0, _k - 1, 0.0, GRB.CONTINUOUS, "S"); // Number of interrupted job pairs
                L = _model.AddVar(0.0, _k - 1, 0.0, GRB.CONTINUOUS, "L"); // Longest time a cable resides in storage
                M = _model.AddVar(0.0, _k - 1, 0.0, GRB.CONTINUOUS, "M"); // Maximum number of cables stored simultaneously
                N = _model.AddVar(0.0, _k - 1, 0.0, GRB.CONTINUOUS, "N"); // Number of violated soft atomic constraints

                pfc = CreatePermutationVariable();


                // objective
                var objExpr = new GRBQuadExpr();
                objExpr.AddTerm(Math.Pow(_k, 3), S);
                objExpr.AddTerm(Math.Pow(_k, 2), M);
                objExpr.AddTerm(Math.Pow(_k, 1), L);
                objExpr.AddTerm(Math.Pow(_k, 0), N);

                _model.SetObjective(objExpr);


                _model.Parameters.TimeLimit = 300;  // 300 seconds
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        protected GRBVar[] CreatePermutationVariable()
        {
            var ub = new double[_k];
            var obj = new double[_k];
            var type = new char[_k];
            for (var i = 0; i < _k; i++)
            {
                ub[i] = _k - 1;
                obj[i] = 0;
                type[i] = GRB.INTEGER;
            }

            return _model.AddVars(null, ub, obj, type, null, 0, _k);
        }

        private List<int> ExtractChamberSequence(GRBVar[] pfc)
        {
            var chamberSequence = new int[_constraintModel.K];

            for (var i = 0; i < chamberSequence.Length; i++)
            {
                chamberSequence[(int)Math.Round(pfc[i].X)] = i + 1;
            }

            return chamberSequence.ToList();
        }
    }
}
    