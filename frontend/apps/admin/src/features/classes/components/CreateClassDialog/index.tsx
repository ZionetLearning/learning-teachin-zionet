import { useState } from "react";
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  TextField,
} from "@mui/material";

import { useCreateClass } from "@admin/api";
import { useTranslation } from "react-i18next";

type Props = {
  open: boolean;
  onClose: () => void;
};

export const CreateClassDialog = ({ open, onClose }: Props) => {
  const { t } = useTranslation();
  const { mutate: createClass } = useCreateClass();
  const [name, setName] = useState("");
  const [description, setDescription] = useState<string>("");

  const handleCreate = () => {
    if (!name.trim()) return;
    createClass(
      { name: name.trim(), description: description.trim() || null },
      {
        onSuccess: () => {
          setName("");
          setDescription("");
          onClose();
        },
      },
    );
  };

  const handleClose = () => {
    setName("");
    setDescription("");
    onClose();
  };

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>{t("pages.classes.createClass")}</DialogTitle>
      <DialogContent>
        <Stack gap={2} sx={{ mt: 1 }}>
          <TextField
            label={t("pages.classes.className")}
            value={name}
            onChange={(e) => setName(e.target.value)}
            autoFocus
          />
          <TextField
            label={t("pages.classes.descriptionOptional")}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            multiline
            minRows={2}
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose} color="inherit">
          {t("pages.classes.cancel")}
        </Button>
        <Button onClick={handleCreate} disabled={!name} variant="contained">
          {t("pages.classes.create")}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
