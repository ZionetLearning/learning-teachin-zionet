import { describe, it, expect } from 'vitest';
import { formatPopulation, languagesList, primaryCurrency } from '../utils/format';

const country = {
  population: 125_800_000,
  languages: { jpn: 'Japanese', ainu: 'Ainu' },
  currencies: { JPY: { name: 'Japanese yen', symbol: '¥' } },
} as any;

describe('format utils', () => {
  it('formatPopulation', () => {
    expect(formatPopulation(0)).toBe('0');
    expect(formatPopulation(1234)).toBe('1,234');
    expect(formatPopulation(country.population)).toMatch(/125,800,000/);
  });

  it('languagesList', () => {
    expect(languagesList({ languages: undefined } as any)).toBe('—');
    expect(languagesList(country)).toBe('Japanese, Ainu');
  });

  it('primaryCurrency', () => {
    expect(primaryCurrency({ currencies: undefined } as any)).toBe('—');
    expect(primaryCurrency(country)).toBe('Japanese yen (¥)');
  });
});
