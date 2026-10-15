from pathlib import Path
import json, math
p=Path('outputs/UnityPort/WorldPacked/manifest.bytes')
m=json.loads(p.read_text(encoding='utf8'))
assign={'vault120_service_pistol':'police_hq','closing_bell':'wall_street','assault_rifle':'park_ave_armory','hunting_rifle':'belvedere','sniper_rifle':'empire_state','combat_shotgun':'macys','sawed_off_shotgun':'subway','laser_rifle':'rockefeller','manhattan_project':'un_hq','plasma_pistol':'con_ed_tower','plasma_rifle':'un_assembly','alien_blaster':'amnh','lady_libertys_torch':'liberty','minigun':'brooklyn_bridge','gatling_laser':'un_hq','missile_launcher':'dumbo','fat_man':'liberty','broadway_slugger':'lyceum_theatre','super_sledge':'domino_sugar','power_fist':'park_ave_armory','brass_knuckles':'katz_deli','torchblade':'st_patricks','scrapshot_launcher':'nabisco_factory','stinger_dart_gun':'smallpox_hospital','spikedriver':'penn_station'}
report=[];used=set()
for wid,site in assign.items():
    loc=next(l for l in m['locations'] if l['id']==site)
    distance=lambda c:math.hypot(c['pos']['x']-loc['x'],c['pos']['z']-loc['z'])
    nearby=[c for c in m['containers'] if c['locked']==0 and c.get('pos') and distance(c)<250]
    assert nearby,(wid,site)
    c=min(nearby,key=lambda c:distance(c)+(100 if c['id'] in used else 0))
    used.add(c['id']);d=next(d for d in m['items'] if d['id']==wid)
    if not any(s['id']==wid for s in c['items']):
        c['items'].append({'id':wid,'count':1})
        ammo=d.get('weapon',{}).get('ammo');mag=d.get('weapon',{}).get('magSize',0)
        if ammo and mag:c['items'].append({'id':ammo,'count':max(1,mag)*2})
    report.append({'weapon':wid,'site':site,'container':c['id'],'distance':round(distance(c),1),'position':c['pos']})
weaponids={d['id'] for d in m['items'] if d['type']=='weapon'}
placed={s['id'] for c in m['containers'] for s in c['items']}
assert weaponids<=placed,weaponids-placed
p.write_text(json.dumps(m,separators=(',',':')),encoding='utf8')
Path('work/unity-port/extra-world-loot.json').write_text(json.dumps(report,indent=2),encoding='utf8')
print(f'All {len(weaponids)} weapon types available in city containers; {len(set(assign.values()))} additional cache sites.')
