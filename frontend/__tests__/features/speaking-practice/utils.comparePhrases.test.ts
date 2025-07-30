import { comparePhrases } from "@/features/practice/speaking-practice/utils/comparePhrases";

describe("comparePhrases()", () => {
  it("returns true for exact match", () => {
    expect(comparePhrases("שלום", "שלום")).toBe(true);
  });

  it("ignores punctuation and whitespace", () => {
    expect(comparePhrases("שלום!", "שלום")).toBe(true);
    expect(comparePhrases("  שלום  ", "שלום")).toBe(true);
  });

  it("is case-sensitive for non-Hebrew but strips diacritics", () => {
    expect(comparePhrases("שָׁלוֹם", "שלום")).toBe(true);
    expect(comparePhrases("Hello", "hello")).toBe(false);
  });

  it("returns false for different phrases", () => {
    expect(comparePhrases("שלום", "שלום עולם")).toBe(false);
  });

  it("handles empty strings", () => {
    expect(comparePhrases("", "")).toBe(true);
    expect(comparePhrases("", "שלום")).toBe(false);
    expect(comparePhrases("שלום", "")).toBe(false);
  });

  it("ignores punctuation in the middle of phrases", () => {
    expect(comparePhrases("שלום, חבר!", "שלום חבר")).toBe(true);
    expect(comparePhrases("שלום-חבר", "שלום חבר")).toBe(true);
  });
});
