export const comparePhrases = (
  userPhrase: string,
  targetPhrase: string,
): boolean => {
  const normalize = (str: string = "") =>
    str
      .normalize("NFD")
      .replace(/\p{M}/gu, "")
      .replace(/[^\p{L}\p{N}]+/gu, " ")
      .trim();

  return normalize(userPhrase) === normalize(targetPhrase);
};
