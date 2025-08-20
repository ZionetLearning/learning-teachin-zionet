import {
  Box,
  Typography,
  Paper,
  Stack,
  TextField,
  Button,
} from "@mui/material";
import { useTranslation } from "react-i18next";
import { useSignalR } from "@/hooks";
import { usePostTask } from "@/api";
import { useState } from "react";

export const SignalRPage = () => {
  const { t } = useTranslation();
  const { status, userId } = useSignalR();
  const { mutate: postTask, isPending } = usePostTask();

  const [id, setId] = useState<string>("1");
  const [name, setName] = useState<string>("");
  const [payload, setPayload] = useState<string>("");

  const idNum = Number(id);
  const isValid =
    Number.isFinite(idNum) &&
    idNum > 0 &&
    name.trim().length > 0 &&
    payload.trim().length > 0;

  const handleSubmit: React.FormEventHandler<HTMLFormElement> = (e) => {
    e.preventDefault();
    if (!isValid) return;

    postTask(
      { id: idNum, name: name.trim(), payload: payload.trim() },
      {
        onSuccess: () => {
          // reset form after success
          setId("1");
          setName("");
          setPayload("");
        },
      }
    );
  };

  return (
    <Box
      sx={{
        height: "100%",
        display: "flex",
        alignItems: "center", // use "center" for vertical centering
        justifyContent: "center", // horizontal centering

        flexDirection: "column",
      }}
    >
      <Typography variant="h4" gutterBottom>
        {t("pages.signalR.title")}
      </Typography>

      <Typography variant="h5" gutterBottom>
        {t("pages.signalR.description")}
      </Typography>

      <Typography variant="h6">
        SignalR status:{" "}
        <b style={{ color: status === "connected" ? "green" : "red" }}>
          {status}
        </b>
        <br />
        userId: {userId}
      </Typography>

      <Paper
        component="form"
        onSubmit={handleSubmit}
        sx={{ mt: 2, p: 2, maxWidth: 520, display: "block" }}
      >
        <Typography variant="h6" gutterBottom>
          {t("pages.signalR.createTask")}
        </Typography>

        <Stack spacing={2}>
          <TextField
            label={t("pages.signalR.taskId")}
            value={id}
            onChange={(e) => setId(e.target.value)}
            required
            fullWidth
          />

          <TextField
            label={t("pages.signalR.taskName")}
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
            fullWidth
          />

          <TextField
            label={t("pages.signalR.payload")}
            value={payload}
            onChange={(e) => setPayload(e.target.value)}
            required
            fullWidth
            multiline
            minRows={2}
          />

          <Stack direction="row" spacing={1} justifyContent="flex-end">
            <Button
              variant="contained"
              type="submit"
              disabled={isPending || !isValid || status !== "connected"}
            >
              {isPending ? t("pages.signalR.posting") : t("pages.signalR.send")}
            </Button>
          </Stack>
        </Stack>
      </Paper>
    </Box>
  );
};
