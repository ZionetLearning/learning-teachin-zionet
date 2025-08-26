import type { Country } from "../api";

export const formatPopulation = (n?: number) => {
  if (!n && n !== 0) return "—";
  return n.toLocaleString();
};

export const primaryCurrency = (country: Country): string => {
  const c = country.currencies
    ? Object.values(country.currencies)[0]
    : undefined;
  return c ? `${c.name}${c.symbol ? ` (${c.symbol})` : ""}` : "—";
};

export const languagesList = (country: Country): string => {
  return country.languages ? Object.values(country.languages).join(", ") : "—";
};
