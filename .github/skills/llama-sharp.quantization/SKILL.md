---
name: llama-sharp.quantization
description: Key concepts and trade-offs for model quantization in LlamaSharp-style stacks.
---

# Purpose

Summarize quantization options, trade-offs, and integration guidance for using quantized GGUF/LLM artifacts with
LlamaSharp-style runtimes.

# When to use

- Reducing memory footprint for large models to enable CPU or limited-GPU deployments.
- Improving throughput at the cost of potential small accuracy regressions.

# Key concepts

- Formats: 8-bit, 4-bit, and mixed-precision schemes.
- Trade-offs: lower-bit quant reduces memory and cache pressure but can degrade accuracy.
- Tooling: quantize offline and store metadata (bits, algorithm, checksum) for validation.
- Runtime: dequantization strategies, CPU vs GPU support, alignment and memory layout.

# Guidance

- Keep the original full-precision model artifact and produce quantized variants with explicit metadata.
- Validate quantized models on representative workloads and include checksums/validation artifacts.
- Prefer mixed-precision or per-layer quantization for sensitive layers when available.

# Example

```csharp
var quantModel = Quantizer.Quantize(fpModel, bits: 4);
var model = Model.Load(quantModel);
```

# Integration notes

- Document quantization parameters with the artifact for reproducibility and future troubleshooting.

# References

- Quantization tutorial: https://scisharp.github.io/LLamaSharp/0.25.0/Tutorials/Quantization/
- Quantize model example: https://scisharp.github.io/LLamaSharp/0.25.0/Examples/QuantizeModel/
- GGUF / model format guidance: https://scisharp.github.io/LLamaSharp/0.25.0/QuickStart/
