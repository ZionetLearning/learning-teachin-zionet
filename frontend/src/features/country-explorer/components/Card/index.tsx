import React from "react";
import { useTranslation } from "react-i18next";
import { useStyles } from "./style";
import type { Country } from "../../api";
import {
  formatPopulation,
  languagesList,
  primaryCurrency,
} from "../../utils/format";

type Props = {
  country: Country;
};

export const Card = ({ country }: Props) => {
  const { t } = useTranslation();
  const classes = useStyles();
  const flag = country.flags?.svg || country.flags?.png;
  return (
    <div className={classes.container}>
      {flag && (
        <img
          className={classes.img}
          src={flag}
          alt={country.flags?.alt || `${country.name.common} flag`}
        />
      )}

      <div className={classes.textContainer}>
        <h3 className={classes.countryName}>
          {country.name.common}{" "}
          {country.cca2 && <small>({country.cca2})</small>}
        </h3>
        <div className={classes.details}>
          <div>
            <strong>{t("pages.countryExplorer.capital")}</strong>{" "}
            {country.capital?.[0] ?? "—"}
          </div>
          <div>
            <strong>{t("pages.countryExplorer.region")}</strong>{" "}
            {country.region ?? "—"}
          </div>
          <div>
            <strong>{t("pages.countryExplorer.population")}</strong>{" "}
            {formatPopulation(country.population)}
          </div>
          <div>
            <strong>{t("pages.countryExplorer.currency")}</strong>{" "}
            {primaryCurrency(country)}
          </div>
          <div>
            <strong>{t("pages.countryExplorer.languages")}</strong>{" "}
            {languagesList(country)}
          </div>
        </div>
      </div>
    </div>
  );
};
