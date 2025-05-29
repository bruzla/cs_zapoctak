# README

## Introduction

**DplyrSharp** is a C# library for fluent and expressive tabular data manipulation, inspired by R’s [dplyr](https://dplyr.tidyverse.org/). It provides a `DataFrame` abstraction and a suite of operations such as `Filter`, `Select`, `Mutate`, `GroupBy`, `Summarise`, and various types of joins. The API supports clean, chainable expressions designed for readability and flexibility. This project was developed as the final assignment for NPRG035 + NPRG038.

## Directory Structure

```
root/
├── README.md                 # Project overview
│
├── docs/
│   └── Doxyfile              # Configuration for generating documentation with Doxygen
│
├── DplyrSharp/               
│   ├── Core/...              # Core data structures and logic
│   │
│   └── IO/...                # Input/output functionality
│
├── ExampleProgram/...        # Sample project that uses the DplyrSharp library
│
└── misc/...                  # Miscellaneous resources
```

## Documentation

The project documentation is generated using Doxygen.
To generate the documentation, ensure you have Doxygen installed, then navigate to the project root and run:

```bash
doxygen docs/Doxyfile
```

The generated documentation can be then accessed via `docs/html/index.html`.

## Honorable Mentions

Huge thanks to the [dplyr](https://dplyr.tidyverse.org/) team and community for inspiring the design, API patterns, and semantics of this project.

Star Wars data used in the example program was retrieved from the [SWAPI API](https://swapi.py4e.com/). Downloaded CSV files can be found in the `misc/data/` folder.