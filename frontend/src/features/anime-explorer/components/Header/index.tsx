import { useEffect, useState } from "react";

import { useTranslation } from "react-i18next";

import { useDebounceValue } from "../../utils";

import { useStyles } from "./style";

export const Header = ({
  initial = "",
  onDebouncedChange,
}: {
  initial?: string;
  onDebouncedChange: (v: string) => void;
}) => {
  const { t } = useTranslation();
  const classes = useStyles();
  const [local, setLocal] = useState(initial);
  const debounced = useDebounceValue(local, 600);

  useEffect(
    function handleDebouncedChange() {
      onDebouncedChange(debounced);
    },
    [debounced, onDebouncedChange],
  );

  return (
    <div className={classes.header}>
      <div className={classes.headerInner}>
        <div className={classes.title}>
          {t("pages.animeExplorer.exploreAnime")}
        </div>
        <div className={classes.controls}>
          <input
            name="search"
            className={classes.searchInput}
            aria-label="Search anime"
            type="text"
            value={local}
            onChange={(e) => setLocal(e.target.value)}
            placeholder={t("pages.animeExplorer.searchByTitle")}
          />
          <button
            className={classes.clearButton}
            style={{ visibility: local ? "visible" : "hidden" }}
            onClick={() => setLocal("")}
            aria-label="Clear search"
          >
            {t("pages.animeExplorer.clear")}
          </button>
        </div>
      </div>
    </div>
  );
};
