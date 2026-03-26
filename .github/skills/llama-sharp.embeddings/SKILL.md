---
name: llama-sharp.embeddings
description: Guidance for generating and using embeddings with LlamaSharp-style tools.
---

# Purpose

Explain practical patterns for generating, storing, and using embeddings with LlamaSharp-style models and downstream
systems (vector DBs, RAG pipelines).

# When to use

- Building a retrieval layer (RAG), semantic search, clustering, or vector similarity features.

# Key concepts

- Embedding generation: model selection, batching, and deterministic preprocessing.
- Postprocessing: normalization, dimensionality checks, and optional PCA/quantization.
- Storage: vector DB choices, indexing strategies, and metadata pairing.
- Retrieval: similarity metrics (cosine/dot) and reranking.

# Guidance

- Batch embeddings for throughput and preserve input ordering for correct mapping to IDs.
- Store both raw vectors and metadata needed to rebuild prompt context or attribution.

# Example

```csharp
var vectors = EmbeddingModel.EncodeBatch(docs);
db.Store(docIds, vectors, metadata);
var qvec = EmbeddingModel.Encode(query);
var hits = db.Search(qvec, topK: 10);
```

## Notes

- Test embedding stability across model updates; small changes can shift distributions.
- Consider dimensionality reduction for large-scale storage to save space.

# References

- Get embeddings tutorial: https://scisharp.github.io/LLamaSharp/0.25.0/Tutorials/GetEmbeddings/
- Embeddings examples: https://scisharp.github.io/LLamaSharp/0.25.0/Examples/GetEmbeddings/
- LLamaSharp overview: https://scisharp.github.io/LLamaSharp/0.25.0/
