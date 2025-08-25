import { describe, it, expect } from "vitest";
import {
  formatPopulation,
  languagesList,
  primaryCurrency,
} from "../utils/format";
import type { Country } from "../api";

const country: Country = {
  cca2: "JP",
  name: { common: "Japan" },
  population: 125_800_000,
  languages: { jpn: "Japanese", ainu: "Ainu" },
  currencies: { JPY: { name: "Japanese yen", symbol: "¥" } },
};

const countryWithoutLanguages: Country = {
  ...country,
  languages: undefined,
};

const countryWithoutCurrencies: Country = {
  ...country,
  currencies: undefined,
};

describe("format utils", () => {
  it("formatPopulation", () => {
    expect(formatPopulation(0)).toBe("0");
    expect(formatPopulation(1234)).toBe("1,234");
    expect(formatPopulation(country.population)).toMatch(/125,800,000/);
  });

  it("languagesList", () => {
    expect(languagesList(countryWithoutLanguages)).toBe("—");
    expect(languagesList(country)).toBe("Japanese, Ainu");
  });

  it("primaryCurrency", () => {
    expect(primaryCurrency(countryWithoutCurrencies)).toBe("—");
    expect(primaryCurrency(country)).toBe("Japanese yen (¥)");
  });
});
