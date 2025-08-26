const BASE = "https://restcountries.com/v3.1";

export type Country = {
  cca2: string; // 2-letter code
  name: { common: string };
  flags?: { svg?: string; png?: string; alt?: string };
  capital?: string[];
  region?: string;
  population?: number;
  currencies?: Record<string, { name: string; symbol?: string }>;
  languages?: Record<string, string>;
};

// Minimal fields to keep payload small
const FIELDS =
  "fields=name,cca2,flags,capital,region,population,currencies,languages";

export type CountryQueryParams = {
  search?: string; // text to search in country name
  region?: string | "All"; // region value for server fetch
};

// Fetch by region when available (smaller result set), otherwise fetch all
export const fetchCountries = async (
  params: CountryQueryParams,
): Promise<Country[]> => {
  const region =
    params.region && params.region !== "All" ? params.region : null;
  const url = region
    ? `${BASE}/region/${encodeURIComponent(region)}?${FIELDS}`
    : `${BASE}/all?${FIELDS}`;

  const res = await fetch(url);
  if (!res.ok) {
    throw new Error(`Countries API error: ${res.statusText}`);
  }
  return res.json();
};
