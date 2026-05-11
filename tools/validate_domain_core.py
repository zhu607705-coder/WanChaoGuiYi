#!/usr/bin/env python3
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
SCRIPTS = ROOT / "My project" / "Assets" / "Scripts"
DOMAIN = SCRIPTS / "Domain"

FORBIDDEN_DOMAIN_TOKENS = [
    "using UnityEngine",
    "MonoBehaviour",
    "SerializeField",
    "GetComponent",
    "gameObject",
    "Mathf.",
    "MapGraph ",
    "MapGraph)",
]

EXPECTED_ADAPTERS = {
    SCRIPTS / "Military" / "ArmyMovementSystem.cs": "DomainArmyMovementSystem",
    SCRIPTS / "Military" / "BattleSimulationSystem.cs": "DomainBattleSimulationSystem",
    SCRIPTS / "Military" / "EngagementDetector.cs": "DomainEngagementDetector",
    SCRIPTS / "Military" / "MapWarResolutionSystem.cs": "DomainMapWarResolutionSystem",
    SCRIPTS / "Military" / "OccupationSystem.cs": "DomainOccupationSystem",
    SCRIPTS / "Governance" / "GovernanceImpactSystem.cs": "DomainGovernanceImpactSystem",
    SCRIPTS / "Economy" / "EconomySystem.cs": "DomainEconomySystem",
}


def fail(message: str) -> None:
    print(f"[domain-core] ERROR: {message}")
    sys.exit(1)


def validate_domain_folder() -> None:
    if not DOMAIN.exists():
        fail(f"Domain folder missing: {DOMAIN}")

    for path in sorted(DOMAIN.rglob("*.cs")):
        text = path.read_text(encoding="utf-8")
        rel = path.relative_to(ROOT)
        for token in FORBIDDEN_DOMAIN_TOKENS:
            if token in text:
                fail(f"Forbidden token '{token}' in {rel}")


def validate_adapters() -> None:
    for path, domain_type in EXPECTED_ADAPTERS.items():
        if not path.exists():
            fail(f"Adapter file missing: {path.relative_to(ROOT)}")

        text = path.read_text(encoding="utf-8")
        rel = path.relative_to(ROOT)
        if domain_type not in text:
            fail(f"Adapter {rel} does not reference {domain_type}")
        if "using UnityEngine" not in text:
            fail(f"Adapter {rel} should remain an explicit Unity boundary")


def main() -> None:
    validate_domain_folder()
    validate_adapters()
    print("[domain-core] OK: Domain folder is Unity-free and adapters delegate to Domain types.")


if __name__ == "__main__":
    main()
