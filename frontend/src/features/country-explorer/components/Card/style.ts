import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  cardContainer: {
    border: "1px solid #eee",
    borderRadius: 12,
    overflow: "hidden",
    boxShadow: "0 1px 4px rgba(0,0,0,0.06)",
    background: "#fff",
  },
  img: {
    width: "100%",
    height: 140,
    objectFit: "cover",
  },
  textContainer: {
    padding: 12,
  },
  countryName: {
    margin: "4px 0 8px",
  },
  details: {
    fontSize: 14,
    color: "#444",
  },
});
