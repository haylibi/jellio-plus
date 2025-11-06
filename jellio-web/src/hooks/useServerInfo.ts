import { useEffect, useState } from 'react';
import useAccessToken from '@/hooks/useAccessToken.ts';
import { getServerInfo } from '@/services/backendService.ts';
import type { ServerInfo, Maybe } from '@/types';

const useServerInfo = (): Maybe<ServerInfo> => {
  const accessToken = useAccessToken();
  const [serverInfo, setServerInfo] = useState<ServerInfo | null | undefined>();

  useEffect(() => {
    const fetchServerInfo = async (): Promise<void> => {
      try {
        // First try without token (use Jellyfin session cookies)
        var serverInfo = await getServerInfo();
        setServerInfo({
          accessToken: accessToken || '', // Use empty string if no localStorage token
          ...serverInfo,
        });
      } catch (firstError) {
        // If no session cookie, try with localStorage token
        if (accessToken) {
          try {
            var serverInfo = await getServerInfo(accessToken);
            setServerInfo({
              accessToken: accessToken,
              ...serverInfo,
            });
          } catch {
            setServerInfo(null);
          }
        } else {
          setServerInfo(null);
        }
      }
    };
    
    void fetchServerInfo();
  }, [accessToken]);

  return serverInfo;
};

export default useServerInfo;
