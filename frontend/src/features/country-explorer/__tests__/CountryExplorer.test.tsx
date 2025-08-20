import { render, screen, fireEvent } from "@testing-library/react";
import { vi, describe, it, expect, beforeEach } from "vitest";

// Stub i18n so t(key) just returns the key string
vi.mock("react-i18next", () => ({
  useTranslation: () => ({
    t: (k: string) => k,
    i18n: { changeLanguage: vi.fn() },
  }),
}));

import type { CountryQueryParams } from '../api'; // safe to import types


// Mock the data hook so we control the results
const useCountriesQueryMock = vi.fn();
vi.mock("../hooks/useCountriesQuery", () => ({
  useCountriesQuery: (...args: CountryQueryParams[]) => useCountriesQueryMock(...args),
}));

// Import after mocks
import { CountryExplorer } from "..";

type Country = {
  name: { common: string };
  cca2?: string;
  flags?: { svg?: string; png?: string; alt?: string };
  capital?: string[];
  region?: string;
  population?: number;
  currencies?: Record<string, { name?: string; symbol?: string }>;
  languages?: Record<string, string>;
};

// Minimal country factory with sensible defaults
const makeCountry = (over: Partial<Country> = {}): Country => {
  return {
    name: { common: "France" },
    cca2: "FR",
    flags: { svg: "flag.svg", alt: "France flag" },
    capital: ["Paris"],
    region: "Europe",
    population: 67_000_000,
    currencies: { EUR: { name: "Euro", symbol: "€" } },
    languages: { fra: "French" },
    ...over,
  };
};

beforeEach(() => {
  vi.clearAllMocks();
});

describe("<CountryExplorer />", () => {
  it("shows loading state", () => {
    useCountriesQueryMock.mockReturnValue({
      data: undefined,
      isLoading: true,
      isError: false,
      error: null,
    });

    render(<CountryExplorer />);
    expect(
      screen.getByText("pages.countryExplorer.loadingCountries"),
    ).toBeInTheDocument();
  });

  it("shows error state", () => {
    useCountriesQueryMock.mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true,
      error: new Error("Error"),
    });

    render(<CountryExplorer />);
    expect(
      screen.getByText(/pages\.countryExplorer\.failedToLoad/i),
    ).toBeInTheDocument();
    expect(screen.getByText(/Error/)).toBeInTheDocument();
  });

  it("renders cards with country data", () => {
    const c1 = makeCountry({
      name: { common: "Argentina" },
      cca2: "AR",
      region: "Americas",
    });
    const c2 = makeCountry({
      name: { common: "Japan" },
      cca2: "JP",
      region: "Asia",
    });

    useCountriesQueryMock.mockReturnValue({
      data: [c1, c2],
      isLoading: false,
      isError: false,
      error: null,
    });

    render(<CountryExplorer />);

    expect(screen.getByText(/Argentina/)).toBeInTheDocument();
    expect(screen.getByText(/Japan/)).toBeInTheDocument();

    // Capital/Region/Population labels are translated keys (stubbed to key names)
    expect(
      screen.getAllByText("pages.countryExplorer.capital").length,
    ).toBeGreaterThan(0);
    expect(
      screen.getAllByText("pages.countryExplorer.region").length,
    ).toBeGreaterThan(0);
    expect(
      screen.getAllByText("pages.countryExplorer.population").length,
    ).toBeGreaterThan(0);
  });

  it("filters by search term (client-side)", () => {
    const c1 = makeCountry({ name: { common: "Norway" } });
    const c2 = makeCountry({ name: { common: "Nigeria" } });

    useCountriesQueryMock.mockReturnValue({
      data: [c1, c2],
      isLoading: false,
      isError: false,
      error: null,
    });

    render(<CountryExplorer />);

    const input = screen.getByPlaceholderText("pages.countryExplorer.searchByName");
    fireEvent.change(input, { target: { value: "nor" } });

    // Should match Norway only
    expect(screen.getByText("Norway")).toBeInTheDocument();
    expect(screen.queryByText("Nigeria")).toBeNull();
  });

  it("filters by region (client-side) in conjunction with server region param", () => {
    const c1 = makeCountry({ name: { common: "Germany" }, region: "Europe" });
    const c2 = makeCountry({ name: { common: "Kenya" }, region: "Africa" });

    useCountriesQueryMock.mockReturnValue({
      data: [c1, c2],
      isLoading: false,
      isError: false,
      error: null,
    });

    render(<CountryExplorer />);

    // 2 selects: [regionSelect, popSelect]
    const [regionSelect] = screen.getAllByRole(
      "combobox",
    ) as HTMLSelectElement[];

    fireEvent.change(regionSelect, { target: { value: "Europe" } });

    expect(screen.getByText("Germany")).toBeInTheDocument();
    expect(screen.queryByText("Kenya")).not.toBeInTheDocument();
  });

  it("filters by population bucket (client-side)", () => {
    const tiny = makeCountry({
      name: { common: "Monaco" },
      population: 39_000,
    });
    const mid = makeCountry({
      name: { common: "Greece" },
      population: 10_600_000,
    });
    const huge = makeCountry({
      name: { common: "India" },
      population: 1_420_000_000,
    });

    useCountriesQueryMock.mockReturnValue({
      data: [tiny, mid, huge],
      isLoading: false,
      isError: false,
      error: null,
    });

    render(<CountryExplorer />);

    // 2 selects: [regionSelect, popSelect]
    const [, popSelect] = screen.getAllByRole(
      "combobox",
    ) as HTMLSelectElement[];

    // <10M
    fireEvent.change(popSelect, { target: { value: "<10M" } });
    expect(screen.getByText("Monaco")).toBeInTheDocument();
    expect(screen.queryByText("Greece")).not.toBeInTheDocument();
    expect(screen.queryByText("India")).not.toBeInTheDocument();

    // 10M-100M
    fireEvent.change(popSelect, { target: { value: "10M-100M" } });
    expect(screen.getByText("Greece")).toBeInTheDocument();
    expect(screen.queryByText("Monaco")).not.toBeInTheDocument();
    expect(screen.queryByText("India")).not.toBeInTheDocument();

    // >=100M
    fireEvent.change(popSelect, { target: { value: ">=100M" } });
    expect(screen.getByText("India")).toBeInTheDocument();
    expect(screen.queryByText("Monaco")).not.toBeInTheDocument();
    expect(screen.queryByText("Greece")).not.toBeInTheDocument();
  });

  it("matches snapshot (stable render)", () => {
    const c = makeCountry({
      name: { common: "Canada" },
      cca2: "CA",
      region: "Americas",
    });

    useCountriesQueryMock.mockReturnValue({
      data: [c],
      isLoading: false,
      isError: false,
      error: null,
    });

    const { asFragment } = render(<CountryExplorer />);
    expect(asFragment()).toMatchSnapshot();
  });
});
