"""Coleta dados dos endpoints de benchmark da API EncurtadorUfabc.

Executa múltiplas requisições HTTP aos endpoints ``/avl/benchmark`` e
``/hash/benchmark`` para diferentes quantidades de operações e gera dois
arquivos CSV (um por estrutura de dados) com os resultados agregados.

Usage:
    python3 scripts/benchmark-collect.py [opções]

Options:
    --base-url URL       URL base da API (default: http://localhost:5143).
                         Também configurável via variável de ambiente
                         ``BENCH_URL``.
    --operations LIST    Quantidade de operações separadas por vírgula
                         (default: 1000,5000,10000,50000). Também via
                         ``BENCH_OPS``.
    --repetitions N      Número de repetições para cada valor de
                         operações (default: 5). Também via ``BENCH_RUNS``.
    --output-dir DIR     Diretório de saída para os arquivos CSV
                         (default: benchmark-data). Também via
                         ``BENCH_OUT``.
    --timeout S          Timeout HTTP em segundos (default: 120).
    --retries N          Tentativas em caso de falha transiente
                         (default: 3).

Output:
    Dois arquivos CSV são gerados no diretório de saída:

    - ``benchmark_avl.csv``:  resultados das execuções com Árvore AVL.
    - ``benchmark_hash.csv``: resultados das execuções com Tabela Hash.

    Cada arquivo contém as seguintes colunas:

    - ``timestamp``:       instante da coleta (ISO 8601 UTC).
    - ``run``:             número da repetição (1-indexado).
    - ``operations``:      quantidade de operações executadas.
    - ``elementCount``:    elementos na estrutura após inserção.
    - ``putMs``:           tempo da fase de inserção (ms).
    - ``getMs``:           tempo da fase de busca (ms).
    - ``deleteMs``:        tempo da fase de remoção (ms).
    - ``totalMs``:         soma dos tempos das três fases (ms).
    - ``allocatedBytes``:  bytes alocados no heap gerenciado.
    - ``treeHeight``:      altura da árvore AVL (vazio para Hash).
    - ``bucketCount``:     quantidade de baldes da tabela Hash (vazio
                           para AVL).
    - ``loadFactor``:      fator de carga da tabela Hash (vazio para
                           AVL).
    - ``maxChainLength``:  maior cadeia de colisão da tabela Hash
                           (vazio para AVL).

Examples:
    # Coleta padrão (5 repetições para 1000, 5000, 10000, 50000 ops):
    python3 scripts/benchmark-collect.py

    # Operações e repetições personalizadas:
    python3 scripts/benchmark-collect.py --operations 10000,50000,100000 --repetitions 10

    # Configuração via variáveis de ambiente:
    export BENCH_OPS=1000,10000,100000 BENCH_RUNS=3
    python3 scripts/benchmark-collect.py

    # API em porta alternativa:
    python3 scripts/benchmark-collect.py --base-url http://localhost:9999

Dependencies:
    Apenas bibliotecas da stdlib do Python (``argparse``, ``csv``,
    ``json``, ``urllib``, etc.). Nenhum pacote externo é necessário.

Requires:
    A API do EncurtadorUfabc deve estar em execução no endereço
    configurado antes de rodar este script.

Author:
    EncurtadorUfabc
"""

import argparse
import csv
import json
import os
import sys
import time
import urllib.error
import urllib.request
from datetime import datetime, timezone

FIELD_NAMES = [
    "timestamp",
    "run",
    "operations",
    "elementCount",
    "putMs",
    "getMs",
    "deleteMs",
    "totalMs",
    "allocatedBytes",
    "treeHeight",
    "bucketCount",
    "loadFactor",
    "maxChainLength",
]


def env_or(key: str, default: str) -> str:
    return os.environ.get(f"BENCH_{key.upper()}", default)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Coleta dados do benchmark AVL vs Hash e gera dois CSVs."
    )
    parser.add_argument(
        "--base-url",
        default=env_or("url", "http://localhost:5143"),
        help="URL base da API (padrao: %(default)s). Tambem via BENCH_URL.",
    )
    parser.add_argument(
        "--operations",
        default=env_or("ops", "1000,5000,10000,50000"),
        help="Quantidade de operacoes separadas por virgula (padrao: %(default)s). Tambem via BENCH_OPS.",
    )
    parser.add_argument(
        "--repetitions",
        type=int,
        default=int(env_or("runs", "5")),
        help="Quantidade de repeticoes para cada valor de operacoes (padrao: %(default)s). Tambem via BENCH_RUNS.",
    )
    parser.add_argument(
        "--output-dir",
        default=env_or("out", "scripts/data/benchmark/"),
        help="Diretorio de saida para os arquivos CSV (padrao: %(default)s). Tambem via BENCH_OUT.",
    )
    parser.add_argument(
        "--timeout",
        type=int,
        default=120,
        help="Timeout HTTP em segundos (padrao: %(default)s).",
    )
    parser.add_argument(
        "--retries",
        type=int,
        default=3,
        help="Tentativas em caso de falha transiente (padrao: %(default)s).",
    )
    return parser.parse_args()


def fetch_benchmark(url: str, operations: int, timeout: int) -> dict | None:
    full_url = f"{url.rstrip('/')}/benchmark?operations={operations}"

    try:
        with urllib.request.urlopen(full_url, timeout=timeout) as response:
            return json.loads(response.read().decode())

    except urllib.error.HTTPError as exc:
        body = exc.read().decode(errors="replace").strip() or exc.reason
        raise RuntimeError(f"HTTP {exc.code}: {body}") from exc

    except urllib.error.URLError as exc:
        raise RuntimeError(f"Erro de rede: {exc.reason}") from exc

    except json.JSONDecodeError as exc:
        raise RuntimeError(f"JSON invalido: {exc}") from exc


def collect(
    structure: str,
    operations: int,
    repetitions: int,
    base_url: str,
    timeout: int,
    retries: int,
    csv_writer: csv.DictWriter,
) -> None:
    url = f"{base_url.rstrip('/')}/{structure}"
    for run_number in range(1, repetitions + 1):
        for attempt in range(1, retries + 1):
            try:
                data = fetch_benchmark(url, operations, timeout)
                break
            except RuntimeError:
                if attempt == retries:
                    raise
                time.sleep(1 * attempt)

        row = {
            "timestamp": datetime.now(timezone.utc).isoformat(timespec="seconds"),
            "run": run_number,
            "operations": operations,
            "elementCount": data.get("elementCount", ""),
            "putMs": data.get("putMs", ""),
            "getMs": data.get("getMs", ""),
            "deleteMs": data.get("deleteMs", ""),
            "totalMs": data.get("totalMs", ""),
            "allocatedBytes": data.get("allocatedBytes", ""),
            "treeHeight": data.get("treeHeight") or "",
            "bucketCount": data.get("bucketCount") or "",
            "loadFactor": data.get("loadFactor") or "",
            "maxChainLength": data.get("maxChainLength") or "",
        }
        csv_writer.writerow(row)


def main() -> None:
    args = parse_args()

    os.makedirs(args.output_dir, exist_ok=True)

    try:
        ops_list = [int(s.strip()) for s in args.operations.split(",") if s.strip()]
    except ValueError:
        print(
            "Erro: --operations deve ser uma lista de inteiros separados por virgula (ex: 1000,5000,10000)",
            file=sys.stderr,
        )
        sys.exit(1)

    structures = ("avl", "hash")
    files = {s: os.path.join(args.output_dir, f"benchmark_{s}.csv") for s in structures}
    writers: dict[str, csv.DictWriter] = {}
    file_handles: dict[str, object] = {}

    for struct, path in files.items():
        file_exists = os.path.isfile(path)
        fh = open(path, "a", newline="")
        file_handles[struct] = fh

        writer = csv.DictWriter(fh, fieldnames=FIELD_NAMES)

        if not file_exists or os.path.getsize(path) == 0:
            writer.writeheader()

        writers[struct] = writer

    total_runs = len(ops_list) * args.repetitions * len(structures)
    current = 0
    start_time = time.monotonic()

    try:
        for ops in ops_list:
            for struct in structures:
                writer = writers[struct]
                print(
                    f"[{struct.upper()}] Iniciando {ops} operacoes x {args.repetitions} repeticoes...",
                    file=sys.stderr,
                )
                collect(
                    structure=struct,
                    operations=ops,
                    repetitions=args.repetitions,
                    base_url=args.base_url,
                    timeout=args.timeout,
                    retries=args.retries,
                    csv_writer=writer,
                )
                current += args.repetitions
                elapsed = time.monotonic() - start_time
                print(
                    f"  Concluido — progresso: {current}/{total_runs} | decorrido: {elapsed:.1f}s",
                    file=sys.stderr,
                )

        elapsed = time.monotonic() - start_time
        print(
            f"\nColeta finalizada em {elapsed:.1f}s.",
            file=sys.stderr,
        )
        for struct, path in files.items():
            print(f"  {path}", file=sys.stderr)
    finally:
        for fh in file_handles.values():
            fh.close()


if __name__ == "__main__":
    main()
