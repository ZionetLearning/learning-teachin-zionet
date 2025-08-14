export const base64ToBlob = (base64: string, mimeType: string): Blob => {
  const cleanBase64 = base64.replace(/^data:[^;]+;base64,/, "");

  const binaryString = window.atob(cleanBase64);

  const bytes = new Uint8Array(binaryString.length);

  for (let i = 0; i < binaryString.length; i++) {
    bytes[i] = binaryString.charCodeAt(i);
  }

  return new Blob([bytes], { type: mimeType });
};
