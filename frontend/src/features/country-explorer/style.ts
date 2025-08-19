import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  container: {
    margin: "0 auto",
    padding: 50,
    display: "grid",
    gridAutoRows: "max-content",
    rowGap: 16,
    gap: 56,
    height: "100dvh",
    minHeight: "100%",
    overflowY: "auto",
  },
  title: {
    marginBottom: 8,
  },
  description: {
    marginTop: 0,
    color: "#555",
  },
  cardsWrapper: {
    overflowY: "auto",
    paddingRight: 2,
    marginTop: 25,
    marginBottom: 50,
  },
  error: {
    color: "crimson",
  },
  cards: {
    display: "grid",
    gridTemplateColumns: "repeat(auto-fill, minmax(240px, 1fr))",
    gap: 25,
  },
});
