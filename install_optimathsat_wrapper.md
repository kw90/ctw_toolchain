
# Install OMT Python Tools

## Deps: install g++, gcc, make, m4, python header files and static libraries
sudo apt-get update
sudo apt install build-essential
sudo apt-get install m4 python3-pip python3-dev libgmp3-dev


# OptiMathSAT

wget http://optimathsat.disi.unitn.it/releases/optimathsat-1.7.0.1/optimathsat-1.7.0.1-linux-64-bit.tar.gz
tar xzf optimathsat-1.6.4-linux-64-bit.tar.gz
mv optimathsat-1.7.0.1-linux-64-bit optimathsat-1.7.0


# LZip

wget http://download.savannah.gnu.org/releases/lzip/lzip-1.15.tar.gz
tar xvf lzip-1.15.tar.gz
cd lzip-1.15/
./configure --prefix=/usr
make
sudo make install


# GMP

wget https://gmplib.org/download/gmp/gmp-6.2.0.tar.lz
tar xvf gmp-6.2.0.tar.lz
cd gmp-6.2.0/
./configure
make
make check
sudo make install


# OMT Python Examples

git clone https://github.com/PatrickTrentin88/omt_python_examples.git
cp -r optimathsat-1.7.0 omt_python_examples/optimathsat/optimathsat-1.7.0
cd omt_python_examples/
mkdir lib
python3 build.py

# Add to PythonPath
export PYTHONPATH=${PYTHONPATH}:/home/kw/omt_python_examples/include
export PYTHONPATH=${PYTHONPATH}:/home/kw/omt_python_examples/lib
export PATH=$PATH:/home/kw/omt_python_examples/optimathsat/optimathsat-1.7.0/bin
