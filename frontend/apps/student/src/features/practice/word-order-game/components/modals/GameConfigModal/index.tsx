import { useState } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  FormControl,
  FormLabel,
  RadioGroup,
  FormControlLabel,
  Radio,
  FormGroup,
  Checkbox,
  TextField,
  Button,
  Typography,
  Box,
} from "@mui/material";
import { useTranslation } from "react-i18next";
import { DifficultyLevel } from "@student/types";

export interface GameConfig {
  difficulty: DifficultyLevel;
  nikud: boolean;
  count: number;
}

interface GameConfigModalProps {
  open: boolean;
  onClose: () => void;
  onConfirm: (config: GameConfig) => void;
  initialConfig?: GameConfig;
}

export const GameConfigModal = ({
  open,
  onClose,
  onConfirm,
  initialConfig = {
    difficulty: 1,
    nikud: true,
    count: 3,
  },
}: GameConfigModalProps) => {
  const { t } = useTranslation();
  const [config, setConfig] = useState<GameConfig>(initialConfig);

  const handleDifficultyChange = (
    event: React.ChangeEvent<HTMLInputElement>,
  ) => {
    setConfig((prev) => ({
      ...prev,
      difficulty: parseInt(event.target.value) as DifficultyLevel,
    }));
  };

  const handleNikudChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setConfig((prev) => ({
      ...prev,
      nikud: event.target.checked,
    }));
  };

  const handleCountChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = parseInt(event.target.value);
    if (value >= 1 && value <= 10) {
      setConfig((prev) => ({
        ...prev,
        count: value,
      }));
    }
  };

  const handleConfirm = () => {
    onConfirm(config);
    onClose();
  };

  const getDifficultyLabel = (difficulty: DifficultyLevel) => {
    switch (difficulty) {
      case 0:
        return t("pages.wordOrderGame.difficulty.easy");
      case 1:
        return t("pages.wordOrderGame.difficulty.medium");
      case 2:
        return t("pages.wordOrderGame.difficulty.hard");
      default:
        return t("pages.wordOrderGame.difficulty.medium");
    }
  };

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="sm"
      fullWidth
      PaperProps={{
        sx: { borderRadius: 2 },
      }}
    >
      <DialogTitle>
        <Typography variant="h5" component="div">
          {t("pages.wordOrderGame.config.title")}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
          {t("pages.wordOrderGame.config.subtitle")}
        </Typography>
      </DialogTitle>

      <DialogContent>
        <Box sx={{ display: "flex", flexDirection: "column", gap: 3, pt: 1 }}>
          {/* Difficulty Selection */}
          <FormControl component="fieldset">
            <FormLabel component="legend" sx={{ mb: 1 }}>
              <Typography variant="subtitle1" fontWeight="medium">
                {t("pages.wordOrderGame.config.difficulty")}
              </Typography>
            </FormLabel>
            <RadioGroup
              value={config.difficulty.toString()}
              onChange={handleDifficultyChange}
            >
              <FormControlLabel
                value="0"
                control={<Radio />}
                label={
                  <Box>
                    <Typography variant="body1">
                      {getDifficultyLabel(0)}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {t("pages.wordOrderGame.config.difficultyDesc.easy")}
                    </Typography>
                  </Box>
                }
              />
              <FormControlLabel
                value="1"
                control={<Radio />}
                label={
                  <Box>
                    <Typography variant="body1">
                      {getDifficultyLabel(1)}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {t("pages.wordOrderGame.config.difficultyDesc.medium")}
                    </Typography>
                  </Box>
                }
              />
              <FormControlLabel
                value="2"
                control={<Radio />}
                label={
                  <Box>
                    <Typography variant="body1">
                      {getDifficultyLabel(2)}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {t("pages.wordOrderGame.config.difficultyDesc.hard")}
                    </Typography>
                  </Box>
                }
              />
            </RadioGroup>
          </FormControl>

          {/* Nikud Option */}
          <FormControl component="fieldset">
            <FormLabel component="legend" sx={{ mb: 1 }}>
              <Typography variant="subtitle1" fontWeight="medium">
                {t("pages.wordOrderGame.config.options")}
              </Typography>
            </FormLabel>
            <FormGroup>
              <FormControlLabel
                control={
                  <Checkbox
                    checked={config.nikud}
                    onChange={handleNikudChange}
                  />
                }
                label={
                  <Box>
                    <Typography variant="body1">
                      {t("pages.wordOrderGame.config.nikud")}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {t("pages.wordOrderGame.config.nikudDesc")}
                    </Typography>
                  </Box>
                }
              />
            </FormGroup>
          </FormControl>

          {/* Sentence Count */}
          <FormControl>
            <FormLabel sx={{ mb: 1 }}>
              <Typography variant="subtitle1" fontWeight="medium">
                {t("pages.wordOrderGame.config.sentenceCount")}
              </Typography>
            </FormLabel>
            <TextField
              type="number"
              value={config.count}
              onChange={handleCountChange}
              helperText={t("pages.wordOrderGame.config.sentenceCountDesc")}
              size="small"
              sx={{ maxWidth: 200 }}
            />
          </FormControl>
        </Box>
      </DialogContent>

      <DialogActions sx={{ px: 3, pb: 3 }}>
        <Button onClick={onClose} color="inherit">
          {t("pages.wordOrderGame.cancel")}
        </Button>
        <Button
          onClick={handleConfirm}
          variant="contained"
          color="primary"
          sx={{ minWidth: 120 }}
        >
          {t("pages.wordOrderGame.config.startGame")}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
