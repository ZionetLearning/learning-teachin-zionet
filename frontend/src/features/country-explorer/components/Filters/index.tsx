import React from "react";
import { useTranslation } from "react-i18next";
import { useStyles } from "./style";
export type PopRangeKey = "ALL" | "<10M" | "10M-100M" | ">=100M";

export type FiltersState = {
  search: string;
  region:
    | "All"
    | "Africa"
    | "Americas"
    | "Asia"
    | "Europe"
    | "Oceania"
    | "Antarctic";
  popRange: PopRangeKey;
};

type Props = {
  value: FiltersState;
  onChange: (next: FiltersState) => void;
};

const regions: FiltersState["region"][] = [
  "All",
  "Africa",
  "Americas",
  "Asia",
  "Europe",
  "Oceania",
  "Antarctic",
];

const popRanges: { key: PopRangeKey; label: string }[] = [
  { key: "ALL", label: "All populations" },
  { key: "<10M", label: "< 10M" },
  { key: "10M-100M", label: "10M – 100M" },
  { key: ">=100M", label: "≥ 100M" },
];

export const Filters = ({ value, onChange }: Props) => {
  const { t } = useTranslation();
  const classes = useStyles();
  function update<K extends keyof FiltersState>(key: K, v: FiltersState[K]) {
    onChange({ ...value, [key]: v });
  }

  return (
    <div className={classes.wrapper}>
      <div>
        <label className={classes.label}>
          {t("pages.countryExplorer.searchByName")}
        </label>
        <input
          className={classes.input}
          placeholder={t("pages.countryExplorer.searchByName")}
          value={value.search}
          onChange={(e) => update("search", e.target.value)}
        />
      </div>

      <div>
        <label className={classes.label}>
          {t("pages.countryExplorer.regionWithNoColon")}
        </label>
        <select
          className={classes.select}
          value={value.region}
          onChange={(e) =>
            update("region", e.target.value as FiltersState["region"])
          }
        >
          {regions.map((r) => (
            <option key={r} value={r}>
              {r}
            </option>
          ))}
        </select>
      </div>

      <div>
        <label style={{ display: "block", fontSize: 12, color: "#555" }}>
          {t("pages.countryExplorer.populationWithNoColon")}
        </label>
        <select
          className={classes.select}
          value={value.popRange}
          onChange={(e) => update("popRange", e.target.value as PopRangeKey)}
        >
          {popRanges.map((p) => (
            <option key={p.key} value={p.key}>
              {p.label}
            </option>
          ))}
        </select>
      </div>
    </div>
  );
};
