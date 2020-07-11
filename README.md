# Translation Pipelines for DZN/FZN/SMT2 Conversion

Python scripts to translate `dzn` data and `mzn` models into flattened `fzn` or
further to `smt2` data/models combos.

## How to Use

Build the required environment using
[`repo2docker`](https://repo2docker.readthedocs.io/en/latest/install.html) by
running the command

```zsh
jupyter-repo2docker --volume $(pwd):/home/$USER/work .
```

That's it! This will install all required packages and launch a Jupyter notebook
server that can be accessed by any browser at
[http://localhost:8888](http://localhost:8888) and using the `TOKEN` printed at
the end of the build process.

Or use Binder
[![Binder](https://mybinder.org/badge_logo.svg)](https://mybinder.org/v2/gh/kw90/ctw_translation_toolchain/master)

### Build it Yourself

In case you would not like to use `repo2docker` or Binder you can install the
required dependencies following the `postBuild` file containing installation
instructions for Linux.

#### Dependencies

The following dependencies are required:

- Python (v3.7)
- CMake (v3.17.0)
- [Miniconda](https://docs.conda.io/projects/conda/en/latest/user-guide/install/) (v4.7.12.1)
- [JupyterLab](https://jupyter.org/install) (v1.2.6)
- [MiniZinc](https://github.com/MiniZinc/MiniZincIDE/releases/download/2.4.3/MiniZincIDE-2.4.3-bundle-linux-x86_64.tgz) (v2.4.3)
  - with [smt2](http://optimathsat.disi.unitn.it/data/smt2.tar.gz) extension (if translating to SMT2)
- [fzn2omt](https://github.com/PatrickTrentin88/fzn2omt) GitHub repo
- [OptiMathSAT](http://optimathsat.disi.unitn.it/releases/optimathsat-1.6.4/optimathsat-1.6.4-linux-64-bit.tar.gz) (v1.6.4 if translating for this solver)
- [z3](https://github.com/Z3Prover/z3) GitHub repo (if translating for this solver)

## Run the Translation Pipelines

To run any pipeline with a local volume attached, move to the `work` directory.
With this it is possible to copy the files into the `dzn`, `fzn` and `mzn`
folders that get used by the conversion scripts.

### Translating `dat` to `dzn`

The notebook `TranslateDAT2DZN.ipynb` provides the script to translate `dat`
data files to `dzn` data files.

### Translating `dzn` with `mzn` to flattened `fzn` for OR-Tools

The notebooks `TranslateDZN2FZN.ipynb` and
`TranslateDZN2FZN-NewDualModels.ipynb` provide the necessary scripts to
translate `dzn` data and `mzn` models to `fzn` files suitable for `OR-Tools`.
The two notebooks only differ in the `mzn` models used.

### Translating `dzn` with `mzn` to `smt2` for z3

The notebook `TranslateDZN2SMT2_Z3.ipynb` provides all scripts necessary to
translate `dzn` data and `mzn` models to `smt2` files suitable for `z3`. It uses
the `smt2` support library for global constraints in the process of generating
intermediary `fzn` files. The generated `smt2` files from are then further
processed. First, the script adds lower and upper bounds for the decision
variable `pfc`. Second, the amount of cavities `k` is added as a workaround to
the files for easier solution extraction.

### Translating `dzn` with `mzn` to `smt2` for OptiMathSAT

The notebook `TranslateDZN2SMT2_OptiMathSAT.ipynb` provides all scripts
necessary to translate `dzn` data and `mzn` models to `smt2` files suitable for
`OptiMathSAT`. It uses the `smt2` support library for global constraints in the
process of generating intermediary `fzn` files. The generated `smt2` files from
are then further processed, similar to the `z3` script. The first two steps are
identical, but one more 3rd step is added, which removes the final optimization
and output lines. The optimization and output is handled by the Python Wrapper
found on [this GitHub
repository](https://github.com/kw90/omt_python_timeout_wrapper).


## Experimental Results

All the model files that are under `models` were run by a multitude of solvers
with the original or translated data files under `data`. The results of all our
experiments can be found in the [excel
table](results/ctw_benchmarking-solvers_result-summary.xlsx) in the `results`
folder.
