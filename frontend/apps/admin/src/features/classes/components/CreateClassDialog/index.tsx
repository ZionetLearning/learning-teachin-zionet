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

type Props = {
  open: boolean;
  onClose: () => void;
};

export const CreateClassDialog = ({ open, onClose }: Props) => {
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
      <DialogTitle>Create Class</DialogTitle>
      <DialogContent>
        <Stack gap={2} sx={{ mt: 1 }}>
          <TextField
            label="Class name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            autoFocus
          />
          <TextField
            label="Description (optional)"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            multiline
            minRows={2}
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={handleClose} color="inherit">
          Cancel
        </Button>
        <Button onClick={handleCreate} disabled={!name} variant="contained">
          Create
        </Button>
      </DialogActions>
    </Dialog>
  );
};
