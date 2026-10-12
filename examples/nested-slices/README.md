# Nested Slices

StatePie keeps root state as a flat dictionary, so nested slice composition is modeled by:

- path-like slice names (for example app/ui and app/user), and
- nested state objects inside each slice.

This example shows both patterns together.
