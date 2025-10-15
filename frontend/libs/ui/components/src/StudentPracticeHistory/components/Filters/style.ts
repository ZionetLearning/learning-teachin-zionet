import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  filtersRow: {
    margin: "0 0 12px",
    display: "flex",
    gap: 12,
    "@media (max-width: 600px)": {
      flexDirection: "column !important",
      gap: 8,
    },
  },
  filterControl: {
    minWidth: 180,
    "@media (max-width: 600px)": {
      minWidth: "100%",
      width: "100%",
    },
  },
});
