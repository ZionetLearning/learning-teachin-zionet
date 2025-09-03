import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Box, Typography, TextField, Stack } from '@mui/material';
import { Button } from '@ui-components';

export type ProfileProps = {
    firstName: string;
    lastName: string;
    email: string;
    onSave?: (data: { firstName: string; lastName: string; email: string }) => void;
};

export const Profile = ({
    firstName,
    lastName,
    email,
    onSave,
}: ProfileProps) => {
    const { t, i18n } = useTranslation();
    const [fn, setFn] = useState(firstName);
    const [ln, setLn] = useState(lastName);
    const [editing, setEditing] = useState(false);

    const isRTL = i18n.dir() === 'rtl';

    useEffect(() => {
        setFn(firstName);
        setLn(lastName);
    }, [firstName, lastName]);

    const dirty =
        fn.trim() !== firstName.trim() || ln.trim() !== lastName.trim();

    const handleCancel = () => {
        setFn(firstName);
        setLn(lastName);
        setEditing(false);
    };

    const handleSave = () => {
        onSave?.({ firstName: fn, lastName: ln, email });
        setEditing(false);
    };

    return (
        <Box display="flex" flexDirection="column" gap={2}
            justifyContent="center" minHeight="100dvh"
            px={{ xs: 2, sm: 3 }} mx={{ xs: 2, sm: 40 }}>
            <Typography variant="h4" fontWeight={700}>
                {t('pages.profile.title')}
            </Typography>

            <Box
                sx={{
                    border: '1px solid',
                    borderColor: 'divider',
                    borderRadius: 3,
                    p: 3,
                    bgcolor: 'background.paper',
                }}
            >
                <Box mt={-1} mb={2}>
                    <Typography variant="h6">{t('pages.profile.subTitle')}</Typography>
                    <Typography variant="body2" color="text.secondary">
                        {t('pages.profile.secondSubTitle')}
                    </Typography>
                </Box>
                <Box
                    sx={{
                        maxWidth: 560,
                        width: '100%',
                        mx: 'auto',
                    }}
                >
                    <Stack spacing={2}>
                        <Box>
                            <Typography
                                variant="body2"
                                color="text.primary"
                                sx={{
                                    mb: 0.3,
                                    textAlign: isRTL ? 'right' : 'left',
                                    fontWeight: 300
                                }}
                            >
                                {t('pages.profile.firstName')}
                            </Typography>
                            <TextField
                                value={fn}
                                onChange={(e) => setFn(e.target.value)}
                                disabled={!editing}
                                fullWidth
                                sx={{
                                    '& .MuiInputLabel-root': { display: 'none' },
                                    '& .MuiInputBase-root': {
                                        direction: isRTL ? 'rtl' : 'ltr',
                                    },
                                    '&.Mui-disabled': { color: 'text.disabled' },
                                    '&.Mui-error': { color: 'error.main' },
                                }}
                            />
                        </Box>

                        <Box>
                            <Typography
                                variant="body2"
                                color="text.primary"
                                sx={{
                                    mb: 0.3,
                                    textAlign: isRTL ? 'right' : 'left',
                                    fontWeight: 300
                                }}
                            >
                                {t('pages.profile.lastName')}
                            </Typography>
                            <TextField
                                value={ln}
                                onChange={(e) => setLn(e.target.value)}
                                disabled={!editing}
                                fullWidth
                                sx={{
                                    '& .MuiInputLabel-root': { display: 'none' },
                                    '& .MuiInputBase-root': {
                                        direction: isRTL ? 'rtl' : 'ltr',
                                    },
                                    '&.Mui-disabled': { color: 'text.disabled' },
                                    '&.Mui-error': { color: 'error.main' },
                                }}
                            />
                        </Box>

                        <Box>
                            <Typography
                                variant="body2"
                                color="text.primary"
                                sx={{
                                    mb: 0.3,
                                    textAlign: isRTL ? 'right' : 'left',
                                    fontWeight: 100
                                }}
                            >
                                {t('pages.profile.email')}
                            </Typography>
                            <TextField
                                value={email}
                                disabled
                                fullWidth
                                sx={{
                                    '& .MuiInputLabel-root': { display: 'none' },
                                    '& .MuiInputBase-root': {
                                        direction: isRTL ? 'rtl' : 'ltr',
                                    },
                                    '&.Mui-disabled': { color: 'text.disabled' },
                                    '&.Mui-error': { color: 'error.main' },
                                }}
                            />
                            <Typography
                                variant="body2"
                                color="text.disabled"
                                sx={{
                                    mt: 0.2,
                                    mb: 0.5,
                                    textAlign: isRTL ? 'right' : 'left',
                                    fontWeight: 100,
                                    fontSize: 10
                                }}
                            >
                                {t('pages.profile.emailCannotBeChanged')}
                            </Typography>
                        </Box>
                    </Stack>
                </Box>

                <Box mt={2} display="flex" gap={1}>
                    {!editing ? (
                        <Button onClick={() => setEditing(true)}>
                            {t('pages.profile.edit')}
                        </Button>
                    ) : (
                        <>
                            <Button
                                onClick={handleSave}
                                disabled={!dirty}
                            >
                                {t('pages.profile.saveChanges')}
                            </Button>
                            <Button variant="outlined" onClick={handleCancel}>
                                {t('pages.profile.cancel')}
                            </Button>
                        </>
                    )}
                </Box>
            </Box>
        </Box>
    );
};