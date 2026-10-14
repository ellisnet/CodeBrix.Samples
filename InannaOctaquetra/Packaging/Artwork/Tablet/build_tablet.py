import re, json
SUB=str.maketrans('0123456789','₀₁₂₃₄₅₆₇₈₉')
sign_uc={}; values={}
def field(L):
    parts=L.split(None,1)
    return parts[1].strip() if len(parts)>1 else ''
def close(b):
    if not b: return
    for v in b['vals']: values.setdefault(v, []).append((b['name'], b.get('ucun'), b['kind']))
    if b.get('ucun') and b['kind']=='sign': sign_uc.setdefault(b['name'], b['ucun'])
sb=None; fb=None
for L in open('osl.asl',encoding='utf-8'):
    L=L.rstrip('\n')
    if L.startswith('@sign'):
        close(fb); fb=None; close(sb); sb={'name':field(L),'vals':[],'kind':'sign'}
    elif L.startswith('@form'):
        close(fb); fb={'name':field(L),'vals':[],'kind':'form'}
    elif L.startswith('@@'):
        close(fb); fb=None
    elif L.startswith('@end sign'):
        close(fb); fb=None; close(sb); sb=None
    elif L.startswith('@ucun'):
        b=fb or sb
        if b is not None and field(L): b['ucun']=field(L)
    elif L.startswith('@v'):
        b=fb or sb
        v=field(L)
        if b is None or not v or v.startswith('-'): continue
        v=re.sub(r'^%\S+\s+','',v).split()[0].strip('?')
        b['vals'].append(v)

def lookup(tok):
    if tok.isupper() or '.' in tok:
        out=[]
        for part in tok.split('.'):
            k=re.sub(r'(\d+)', lambda m:m.group(1).translate(SUB), part)
            if k not in sign_uc: raise KeyError('name '+k)
            out.append((part, k, sign_uc[k]))
        return out
    t=tok.replace('ḫ','h').replace('ĝ','ŋ')
    m=re.match(r'^(.*?)(\d+)$', t)
    if m: t=m.group(1)+m.group(2).translate(SUB)
    hits=[h for h in values.get(t,[]) if h[2]=='sign' and h[1]] or [h for h in values.get(t,[]) if h[1]]
    if not hits: raise KeyError(t)
    if len({h[0] for h in hits})>1: print('  AMBIGUOUS', tok, hits)
    return [(tok, hits[0][0], hits[0][1])]

tablet = {
 1:"[nam]-niĝ2-si-[sa2 ma-an-šum2]", 2:"iri laḫ5 [ma-an-šum2]", 3:"i-si-iš ĝa2-ĝa2 ma-<an-šum2>",
 4:"šag4 ḫul2-la ma-<an-šum2>", 5:"lul-da ma-<an-šum2>", 6:"kur ki-bal ma-<an-šum2>", 7:"nam-dug3-ge ma-<an-šum2>",
 8:"kaš4 du ma-<an-šum2>", 9:"ki-tuš gen6-na ma-<an-šum2>", 10:"nam-nagar ma-<an-šum2>", 11:"nam-tibira ma-<an-šum2>",
 12:"nam-dub-sar ma-<an-šum2>", 13:"nam-simug ma-<an-šum2>", 14:"nam-ašgab ma-<an-šum2>", 15:"nam-azlag2 ma-<an-šum2>",
 16:"nam-šidim ma-<an-šum2>", 17:"nam-ad-KID ma-<an-šum2>", 18:"ĝeštug2 ma-<an-šum2>", 19:"ĝizzal ma-<an-šum2>",
 20:"šu-luḫ kug-ge ma-<an-šum2>", 21:"e2-X-ra ma-<an-šum2>", 22:"ne-mur dub ma-<an-šum2>", 23:"ĝa2 udu ma-<an-šum2>",
 24:"ni2 teĝ3-ĝe26 ma-<an-šum2>", 25:"niĝ2-me-ĝar ma-<an-šum2>", 26:"kur-ku ma-<an-šum2>", 27:"LUL zu2 sis ma-<an-šum2>",
 28:"izi mu2-mu2 ma-<an-šum2>", 29:"izi ten-ten ma-<an-šum2>", 30:"a2 kuš2-u3 ma-<an-šum2>", 31:"KA.GAN2-ge ma-<an-šum2>",
 32:"im-[ri-a gu2] ĝar-ra [ma-an-šum2]", 33:"[lu-lu]-/bu\\-na [ma-an-šum2]", 34:"du14 mu2-mu2 [ma-an-šum2]",
 35:"u3-ma [ma-an-šum2]", 36:"ad gi4-[gi4 ma-an-šum2]",
}
out=[]
for n in range(1,37):
    src=re.sub(r'-?<[^>]*>','',tablet[n])
    toks=re.findall(r'\[|\]|/|\\|\s+|-|[^\s\[\]/\\-]+', src)
    broken=False; damaged=False; items=[]
    for t in toks:
        if t=='[': broken=True; continue
        if t==']': broken=False; continue
        if t=='/': damaged=True; continue
        if t=='\\': damaged=False; continue
        if t.isspace(): items.append({'k':'space'}); continue
        if t=='-': continue
        if t=='X': items.append({'k':'illegible','r':'X'}); continue
        for part,name,uc in lookup(t):
            for ch in uc:
                items.append({'k':'broken' if broken else ('worn' if damaged else 'sign'),'r':part,'n':name,'c':ch,'cp':f"U+{ord(ch):X}"})
    out.append({'line':n,'etcsl':tablet[n],'items':items})
    seq=' '.join(('['+i['c']+']' if i['k']=='broken' else ('⸢'+i['c']+'⸣' if i['k']=='worn' else i.get('c','x' if i['k']=='illegible' else '·'))) for i in items)
    names=' '.join(i.get('n','x' if i['k']=='illegible' else '|') for i in items)
    print(f"{n:2} {seq}    {names}")
json.dump(out, open('tablet-E.json','w'), ensure_ascii=False, indent=1)
