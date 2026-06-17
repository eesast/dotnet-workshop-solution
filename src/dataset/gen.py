#!/usr/bin/env python3
# -*- coding: utf-8 -*-

from __future__ import annotations

import csv
import json
import random
import uuid
from dataclasses import dataclass
from datetime import datetime, timedelta, timezone
from pathlib import Path
from typing import Dict, List, Tuple, Optional

# 微服务与副本数
SERVICE_REPLICAS: Dict[str, int] = {
    "gateway": 2,
    "userservice": 2,
    "contentservice": 2,
    "emailservice": 3,
    "authservice": 3,
}

# 微服务调用拓扑。
CALL_TOPOLOGY: Dict[str, List[str]] = {
    "gateway": ["userservice", "contentservice", "authservice"],
    "userservice": ["authservice", "emailservice"],
    "contentservice": ["emailservice"],
    "emailservice": [],
    "authservice": [],
}

# 日志数量。
LOG_COUNT = 200

# 起始时间。脚本会保证生成的 timestamp 从小到大。
START_TIME = datetime(2026, 6, 5, 16, 0, 0, tzinfo=timezone.utc)

# 相邻两条日志的时间间隔范围，单位为毫秒。
MIN_TIME_STEP_MS = 5
MAX_TIME_STEP_MS = 2000

# internal 事件出现概率。
INTERNAL_EVENT_PROBABILITY = 0.2

# 输出文件。
OUTPUT_PATH = Path("gen_logs/generated_logs.log")

# 随机种子。设置为 None 表示每次运行结果都不同。
RANDOM_SEED: Optional[int] = None


# ============================================================
# 一般不需要修改下面的代码
# ============================================================

HTTP_METHODS = ["GET", "POST", "PUT", "DELETE"]
API_PATHS = [
    "/api/user/john",
    "/api/user/alice",
    "/api/content/notice",
    "/api/content/article",
    "/api/auth/login",
    "/api/auth/logout",
]
STATUS_CODES = [200, 200, 200, 201, 204, 400, 401, 403, 404, 500]

WARNING_EXCEPTIONS = [
    ("System.TimeoutException", "Request processing is slower than expected."),
    ("System.IO.IOException", "Temporary file cache is unavailable."),
    ("System.Net.Http.HttpRequestException", "Remote service responded slowly."),
]

ERROR_EXCEPTIONS = [
    ("System.InvalidOperationException", "Failed to load service configuration."),
    ("System.NullReferenceException", "Object reference not set to an instance of an object."),
    ("System.Data.DataException", "Database query failed."),
    ("System.UnauthorizedAccessException", "Access to protected resource is denied."),
]


@dataclass(frozen=True)
class CsvLogLine:
    lineno: int
    timestamp: datetime
    pod_name: str
    message: Dict[str, object]

    def to_csv_row(self) -> List[str]:
        return [
            str(self.lineno),
            format_timestamp(self.timestamp),
            self.pod_name,
            json.dumps(self.message, ensure_ascii=False),
        ]


def format_timestamp(value: datetime) -> str:
    """格式化为 2026-06-05T16:00:29.045Z。"""
    value = value.astimezone(timezone.utc)
    milliseconds = value.microsecond // 1000
    return value.strftime("%Y-%m-%dT%H:%M:%S.") + f"{milliseconds:03d}Z"


def choose_pod(service: str) -> str:
    replica_count = SERVICE_REPLICAS[service]
    replica_id = random.randrange(replica_count)
    return f"{service}-{replica_id}"


def choose_service_with_outgoing_edges() -> str:
    candidates = [service for service, targets in CALL_TOPOLOGY.items() if targets]
    if not candidates:
        raise ValueError("CALL_TOPOLOGY 中至少需要有一个服务可以调用其他服务。")
    return random.choice(candidates)


def make_call_event(source_service: str) -> Tuple[str, Dict[str, object]]:
    """生成 call 事件：本服务向 target-service 发起请求。"""
    target_service = random.choice(CALL_TOPOLOGY[source_service])
    message = {
        "severity": "INFO",
        "event": "call",
        "request-id": str(uuid.uuid4()),
        "target-service": target_service,
        "duration-ms": random.randint(1, 300),
    }
    return choose_pod(source_service), message


def make_request_event(service: Optional[str] = None, request_id: Optional[str] = None) -> Tuple[str, Dict[str, object]]:
    """生成 request 事件：本服务收到请求。"""
    if service is None:
        service = random.choice(list(SERVICE_REPLICAS.keys()))

    message = {
        "severity": "INFO",
        "event": "request",
        "request-id": request_id or str(uuid.uuid4()),
        "method": random.choice(HTTP_METHODS),
        "path": random.choice(API_PATHS),
        "status-code": random.choice(STATUS_CODES),
    }
    return choose_pod(service), message


def make_internal_event() -> Tuple[str, Dict[str, object]]:
    """生成 internal 事件：本服务发生内部警告或错误。"""
    service = random.choice(list(SERVICE_REPLICAS.keys()))
    severity = random.choice(["WARNING", "ERROR"])

    if severity == "WARNING":
        exception_name, exception_message = random.choice(WARNING_EXCEPTIONS)
    else:
        exception_name, exception_message = random.choice(ERROR_EXCEPTIONS)

    message = {
        "severity": severity,
        "event": "internal",
        "exception": f"{exception_name}: {exception_message}",
    }
    return choose_pod(service), message


def next_timestamp(current: datetime) -> datetime:
    step_ms = random.randint(MIN_TIME_STEP_MS, MAX_TIME_STEP_MS)
    return current + timedelta(milliseconds=step_ms)


def generate_log_lines(count: int) -> List[CsvLogLine]:
    if count < 0:
        raise ValueError("count 不能为负数。")

    if RANDOM_SEED is not None:
        random.seed(RANDOM_SEED)

    lines: List[CsvLogLine] = []
    current_time = START_TIME

    for lineno in range(count):
        current_time = next_timestamp(current_time)

        if random.random() < INTERNAL_EVENT_PROBABILITY:
            pod_name, message = make_internal_event()
        else:
            # call 和 request 都是 INFO。
            if random.random() < 0.5:
                source_service = choose_service_with_outgoing_edges()
                pod_name, message = make_call_event(source_service)
            else:
                pod_name, message = make_request_event()

        lines.append(CsvLogLine(lineno, current_time, pod_name, message))

    return lines


def write_csv(lines: List[CsvLogLine], output_path: Path) -> None:
    output_path.parent.mkdir(parents=True, exist_ok=True)

    with output_path.open("w", newline="", encoding="utf-8") as file:
        writer = csv.writer(file)
        for line in lines:
            writer.writerow(line.to_csv_row())


def main() -> None:
    lines = generate_log_lines(LOG_COUNT)
    write_csv(lines, OUTPUT_PATH)
    print(f"Generated {len(lines)} log lines: {OUTPUT_PATH}")


if __name__ == "__main__":
    main()
