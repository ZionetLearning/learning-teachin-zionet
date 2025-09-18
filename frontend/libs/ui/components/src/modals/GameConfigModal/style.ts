import { createUseStyles } from "react-jss";

export const useStyles = createUseStyles({
  gameConfigModal: {
    "& .MuiDialog-paper": {
      borderRadius: 16,
    },
  },
  modalTitle: {
    marginTop: 8,
  },
  modalContent: {
    display: "flex",
    flexDirection: "column",
    gap: 24,
    paddingTop: 8,
  },
  formLabel: {
    marginBottom: 8,
  },
  sentenceCountField: {
    maxWidth: 200,
  },
  modalActions: {
    paddingHorizontal: 24,
    paddingBottom: 24,
  },
  startGameButton: {
    minWidth: 120,
  },
});