export const createdEmails = new Set<string>();

export const addCreatedEmail = (email: string) => {
  if (email) createdEmails.add(email);
};
