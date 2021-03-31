# Cable Tree Wiring (CTW) Toolchain

This repository contains the translation and experiment toolchain for [the
paper](https://arxiv.org/abs/2011.12862) "Cable Tree Wiring -- Benchmarking
Solvers on a Real-World Scheduling Problem with a Variety of Precedence
Constraints".

## Translation Toolchain

The folder `translation_toolchain` contains all Python scripts used to translate
`dzn` data and `mzn` models into flattened `fzn` or `smt2` data/models combos.
It also provides a reproducible Binder environment to run the translations and
models on the benchmark data.

See the [README](./translation_toolchain/README.md) for the
`translation_toolchain` or
[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/kw90/ctw_toolchain/fix-binder).

## Minizinc Experiments

The Powershell scripts that were used for solving cable tree instances with the
Minizinc command line tool can be found in the `minizinc_experiments` folder
along with instructions. Check out the
[README](./minizinc_experiments/README.md) for further instructions.

## Experimental Results

All the models were run by a multitude of solvers with the original or
translated data files. The results of the solver benchmark can be found in [the excel
sheet](results/ctw_benchmarking-solvers_result-summary.xlsx) in the results folder.
