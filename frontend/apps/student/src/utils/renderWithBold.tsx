export const renderWithBold = (
  str: string,
): (string | React.ReactElement)[] => {
  if (!str) return [""];
  const parts = str.split(/(<b>.*?<\/b>|\*\*.*?\*\*)/g);

  return parts.map((part, idx) => {
    // Match <b>...</b>
    const htmlMatch = part.match(/^<b>(.*?)<\/b>$/s);
    if (htmlMatch) {
      return (
        <strong key={idx} style={{ fontWeight: 600 }}>
          {htmlMatch[1]}
        </strong>
      );
    }

    // Match **...**
    const mdMatch = part.match(/^\*\*(.*?)\*\*$/s);
    if (mdMatch) {
      return (
        <strong key={idx} style={{ fontWeight: 600 }}>
          {mdMatch[1]}
        </strong>
      );
    }
    return <span key={idx}>{part}</span>;
  });
};
