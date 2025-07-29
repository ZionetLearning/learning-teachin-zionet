import { comparePhrases } from '@/features/speaking-practice/utils/comparePhrases';

// unit test for comparePhrases function
describe('comparePhrases()', () => {
	it('returns true for exact match', () => {
		expect(comparePhrases('שלום', 'שלום')).toBe(true);
	});

	it('ignores punctuation and whitespace', () => {
		expect(comparePhrases('שלום!', 'שלום')).toBe(true);
		expect(comparePhrases('  שלום  ', 'שלום')).toBe(true);
	});

	it('is case-sensitive for non-Hebrew but strips diacritics', () => {
		expect(comparePhrases('שָׁלוֹם', 'שלום')).toBe(true);
		expect(comparePhrases('Hello', 'hello')).toBe(false);
	});

	it('returns false for different phrases', () => {
		expect(comparePhrases('שלום', 'שלום עולם')).toBe(false);
	});
});
