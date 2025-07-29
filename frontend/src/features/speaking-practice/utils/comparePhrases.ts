export const comparePhrases = (
	userPhrase: string,
	targetPhrase: string
): boolean => {
	const normalize = (str: string = '') =>
		str.replace(/[^\p{L}\p{N}\s]/gu, '').trim();

	return normalize(userPhrase) === normalize(targetPhrase);
};
