import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  filtersRow: {
    margin: "0 0 12px",
    display: "flex",
    flexWrap: "wrap",
    alignItems: "center",
    gap: 12,
  },
  filterControl: {
    minWidth: 160,
    "@media (max-width: 900px)": { minWidth: 140 },
    "@media (max-width: 600px)": { minWidth: "100%", width: "100%" },
  },
  dateGroup: {
    display: "flex",
    gap: 12,
    "@media (max-width: 600px)": {
      order: 2,
      width: "100%",
      flexDirection: "column",
    },
  },
  actions: {
    marginLeft: "auto",
    display: "flex",
    alignItems: "center",
    gap: 8,
    "@media (max-width: 600px)": {
      order: 3,
      width: "100%",
      justifyContent: "flex-end",
      marginLeft: 0,
    },
  },
});
