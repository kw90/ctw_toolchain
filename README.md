# Translation Pipelines for DZN/FZN/SMT2 Conversion

Python scripts to translate `dzn` data and `mzn` models into flattened `fzn` or
further to `smt2` data/models combos.

## How to Use

Build the required environment using
[`repo2docker`](https://repo2docker.readthedocs.io/en/latest/install.html) by
running the command

```zsh
jupyter-repo2docker --volume $(pwd):/home/kw/work .
```

That's it! This will install all required packages and launch a Jupyter notebook
server that be accessed by any browser at
[http://localhost:8888](http://localhost:8888) and using the `TOKEN` printed at
the end of the build process.
